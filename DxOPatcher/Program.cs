using System.CommandLine;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Humanizer;
using Spectre.Console;

namespace DxOPatcher
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand =
                new RootCommand("Simple application for patching license activation against DxO PhotoLab.");

            var inputOptions = new Option<DirectoryInfo?>("--input",
                    description: "Target DxO PhotoLab directory (i.e., path containing DxO.PhotoLab.exe).")
            { IsRequired = true };
            var verboseOptions = new Option<bool>("--verbose", description: "Enable verbose logging.");
            var backupOptions = new Option<bool>("--keep", description: "Backup original assembly.");

            rootCommand.AddOption(inputOptions);
            rootCommand.AddOption(verboseOptions);
            rootCommand.AddOption(backupOptions);

            rootCommand.SetHandler((di, shouldVerbose, shouldBackup) =>
            {
                if (di is not { Exists: true })
                {
                    throw new DirectoryNotFoundException("Cannot find the target DxO directory.");
                }

                var activationAssembly = FindActivationAssembly("DxO.PhotoLab.Activation.dll", di);
                var activationInteropAssembly = FindActivationAssembly("DxO.PhotoLab.Activation.Interop.dll", di);
                if (activationAssembly == null || activationInteropAssembly == null)
                {
                    throw new FileNotFoundException("Cannot find the required assembly.");
                }

                PatchActivationAssembly(activationAssembly.FullName, verbose: shouldVerbose, keepOriginal: shouldBackup);
                PatchActivationAssembly(activationInteropAssembly.FullName, verbose: shouldVerbose, keepOriginal: shouldBackup);
            }, inputOptions, verboseOptions, backupOptions);


            return await rootCommand.InvokeAsync(args);
        }

        static FileInfo? FindActivationAssembly(string dllName, DirectoryInfo? directoryInfo)
        {
            // use the backup file if one exists
            var expectedPath = Path.Join(directoryInfo?.FullName, dllName);
            var backupPath = Path.Join(directoryInfo?.FullName, $"{dllName}.bak");
            var backupFi = new FileInfo(backupPath);
            if (backupFi.Exists){
                backupFi.MoveTo(expectedPath, overwrite: true);
            }
            var expectedFi = new FileInfo(expectedPath);
            return expectedFi.Exists ? expectedFi : null;
        }

        static void PatchActivationAssembly(string targetFile, bool verbose, bool keepOriginal)
        {
            if (keepOriginal)
            {
                File.Copy(targetFile, targetFile + ".bak", overwrite: true);
                AnsiConsole.MarkupLine("[green]Backed up original assembly.[/]");
            }
            var moduleContext = ModuleDef.CreateModuleContext();
            var tempOutput = Path.GetTempFileName();
            using (var module = ModuleDefMD.Load(targetFile, moduleContext))
            {
                var patchedList = new List<string>();
                var features = module.Types.Where(x => x.FullName.Contains("DxO.PhotoLab.Activation.Feature") || x.FullName.Contains("DxOActivation"));
                Console.WriteLine(string.Join(',', features));
                foreach (var feature in features)
                {
                    var methodsToPatchToTrue = feature?.Methods.Where(x => x.HasBody).Where(x => x.Name.EndsWith("IsValid") || x.Name.EndsWith("HasAnyLicense") || x.Name.EndsWith("IsActivated") || x.Name.EndsWith("IsElite"));
                    if (methodsToPatchToTrue != null)
                    {
                        foreach (var method in methodsToPatchToTrue)
                        {
                            PatchWithLdcRet(method.FullName, method.Body, 1, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }
                    var methodsToPatchToFalse = feature?.Methods.Where(x=>x.HasBody).Where(x => x.Name.EndsWith("IsExpired") || x.Name.EndsWith("IsDemo") || x.Name.EndsWith("IsTemporary") || x.Name == "Check");
                    if (methodsToPatchToFalse != null)
                    {
                        foreach (var method in methodsToPatchToFalse)
                        {
                            PatchWithLdcRet(method.FullName, method.Body, 0, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }
                    var methodsToPatchToTwo = feature?.Methods.Where(x => x.HasBody).Where(x => x.Name.EndsWith("DemoType"));
                    if (methodsToPatchToTwo != null)
                    {
                        foreach (var method in methodsToPatchToTwo)
                        {
                            PatchWithLdcRet(method.FullName, method.Body, 2, verbose); 
                            patchedList.Add(method.FullName);
                        }
                    }
                    var methodsToPatchToSpecifiedAmount = feature?.Methods.Where(x => x.HasBody).Where(x => x.Name.EndsWith("RemainingDays") || x.Name == "RemainingOfflineDays");
                    if (methodsToPatchToSpecifiedAmount != null)
                    {
                        foreach (var method in methodsToPatchToSpecifiedAmount)
                        {
                            PatchWithLdcRet(method.FullName, method.Body, 99, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }
                }
                AnsiConsole.Status().Spinner(Spinner.Known.Aesthetic)
                    .Start("Saving assembly...", x =>
                    {
                        if (module.IsILOnly)
                        {
                            module.Write(tempOutput);
                        }
                        else
                        {
                            var writerOptions = new NativeModuleWriterOptions(module, false);
                            module.NativeWrite(tempOutput, writerOptions);
                        }
                    });

                var sb = new StringBuilder();
                foreach (var patched in patchedList)
                {
                    sb.AppendLine($"- [green]{patched}[/]");
                }

                var panel = new Panel(sb.ToString());
                panel.Padding = new Padding(1, 1);
                panel.Header("Patched");
                AnsiConsole.Write(panel);
            }

            if (targetFile.EndsWith(".bak"))
            {
                targetFile = targetFile.Replace(".bak", "");
            }
            File.Move(tempOutput, targetFile, overwrite: true);

            AnsiConsole.MarkupLine(
                $"[green]Assembly saved as {targetFile}[/] with file size [bold]{new FileInfo(targetFile).Length.Bytes().Humanize()}[/].");
        }

        static void PatchWithLdcRet(string methodFullName, CilBody cilBody, int ldcValue, bool verbose = false)
        {
            if (verbose){
                AnsiConsole.MarkupLine(
                    $"Located [grey]{methodFullName}[/], patching with [grey]\"return {ldcValue};\"[/].");
            }
            cilBody.Instructions.Clear();
            cilBody.ExceptionHandlers.Clear();
            cilBody.Variables.Clear();
            cilBody.Instructions.Add(Instruction.CreateLdcI4(ldcValue));
            cilBody.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}
