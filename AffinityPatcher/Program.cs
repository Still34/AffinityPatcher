using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Humanizer;
using Spectre.Console;

namespace AffinityPatcher
{
    internal class Program
    {
        // Token: 0x02000502 RID: 1282
        public enum CrashReportUploadPolicy
        {
            // Token: 0x04009C1F RID: 39967
            User,

            // Token: 0x04009C20 RID: 39968
            Always,

            // Token: 0x04009C21 RID: 39969
            Never
        }

        static async Task<int> Main(string[] args)
        {
            var rootCommand =
                new RootCommand("Simple application for patching license activation amongst Affinity v2.x/v1.x products.");

            var inputOptions = new Option<DirectoryInfo?>("--input",
                    description: "Target Affinity directory (i.e., path containing Photo/Designer.exe).")
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
                    throw new DirectoryNotFoundException("Cannot find the target Affinity directory.");
                }

                var personaAssembly = FindPersonaAssembly(di);
                if (personaAssembly == null)
                {
                    throw new FileNotFoundException("Cannot find the required assembly.");
                }

                PatchPersonaAssembly(personaAssembly.FullName, verbose: shouldVerbose, keepOriginal: shouldBackup);
            }, inputOptions, verboseOptions, backupOptions);


            return await rootCommand.InvokeAsync(args);
        }

        static FileInfo? FindPersonaAssembly(DirectoryInfo? directoryInfo)
        {
            var targetPath = Path.Join(directoryInfo?.FullName, "Serif.Interop.Persona.dll");
            var fi = new FileInfo(targetPath);
            if (fi.Exists) return fi;
            // use the backup file if one exists
            targetPath = Path.Join(directoryInfo?.FullName, "Serif.Interop.Persona.dll.bak");
            fi = new FileInfo(targetPath);
            return fi.Exists ? fi : null;
        }

        static void PatchPersonaAssembly(string targetFile, bool verbose, bool keepOriginal)
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
                var application = module.Types.FirstOrDefault(x => x.FullName == "Serif.Interop.Persona.Application");
                var methodsToPatchToTrue = application?.Methods.Where(x =>
                    x.Name == "HasEntitlementToRun" || x.Name == "CheckEula" || x.Name == "CheckAnalytics");
                if (methodsToPatchToTrue != null)
                {
                    foreach (var method in methodsToPatchToTrue)
                    {
                        if (verbose)
                        {
                            AnsiConsole.MarkupLine(
                                $"Located [grey]{method.FullName}[/], patching with [grey]\"return true\"[/].");
                        }

                        PatchWithLdcRet(method.Body, 1);
                        patchedList.Add(method.FullName);
                    }
                }

                var methodsToPatchToFalse = application?.Methods.Where(x => x.Name == "get_AllowsOptInAnalytics");
                if (methodsToPatchToFalse != null)
                {
                    
                    foreach (var method in methodsToPatchToFalse)
                    {
                        if (verbose)
                        {
                            AnsiConsole.MarkupLine(
                                $"Located [grey]{method.FullName}[/], patching with [grey]\"return false\"[/].");
                        }

                        PatchWithLdcRet(method.Body, 0);
                        patchedList.Add(method.FullName);
                    }
                }

                var crashPolicy = application?.Methods.FirstOrDefault(x => x.Name == "GetCrashReportUploadPolicy");
                if (crashPolicy != null)
                {
                    if (verbose)
                    {
                        AnsiConsole.MarkupLine(
                            $"Located [grey]{crashPolicy.FullName}[/], patching as [grey]{CrashReportUploadPolicy.Never.Humanize()}.[/]");
                    }

                    PatchWithLdcRet(crashPolicy.Body, (int)CrashReportUploadPolicy.Never);
                    patchedList.Add(crashPolicy.FullName);
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

        static void PatchWithLdcRet(CilBody cilBody, int ldcValue)
        {
            cilBody.Instructions.Clear();
            cilBody.ExceptionHandlers.Clear();
            cilBody.Variables.Clear();
            cilBody.Instructions.Add(Instruction.CreateLdcI4(ldcValue));
            cilBody.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}