﻿using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text;
using AffinityPatcher.Enums;
using AffinityPatcher.Utils;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Humanizer;
using Spectre.Console;

namespace AffinityPatcher
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand =
                new RootCommand("Universal application patcher for Affinity v2.x/v1.x products and DxO PhotoLab.");

            var inputOptions = new Option<DirectoryInfo?>("--input",
                    description: "Target application directory (i.e., path containing the main executable).")
            { IsRequired = true };

            var modeOptions = new Option<PatcherMode>("--mode",
                    description: "Select which application to patch.")
            { IsRequired = false };
            modeOptions.SetDefaultValue(PatcherMode.Affinity);

            var verboseOptions = new Option<bool>("--verbose", description: "Enable verbose logging.");
            var backupOptions = new Option<bool>("--keep", description: "Backup original assembly.");

            rootCommand.AddOption(inputOptions);
            rootCommand.AddOption(modeOptions);
            rootCommand.AddOption(verboseOptions);
            rootCommand.AddOption(backupOptions);

            rootCommand.SetHandler((di, mode, shouldVerbose, shouldBackup) =>
            {
                if (di is not { Exists: true })
                    throw new DirectoryNotFoundException($"Cannot find the target {mode} directory.");

                bool hasWriteAccess = DirectoryAccessGeneric.HasWriteAccess(di.FullName);
                if (shouldVerbose)
                    AnsiConsole.MarkupLine($"[grey]Checking write access to {di.FullName}: {hasWriteAccess}[/]");
                if (!hasWriteAccess)
                    throw new AccessViolationException("Target directory does not have write access. You may need to re-run this application as administrator.");

                AnsiConsole.MarkupLine(
                    $"[grey]Patching product \"{mode}\" - if this is incorrect, make sure you have selected the desired product via the \"--mode\" switch.[/]");
                switch (mode)
                {
                    case PatcherMode.Affinity:
                        PatchAffinity(di, shouldVerbose, shouldBackup);
                        break;
                    case PatcherMode.DxO:
                        PatchDxO(di, shouldVerbose, shouldBackup);
                        break;
                    default:
                        throw new ArgumentException("Invalid patcher mode specified.");
                }
            }, inputOptions, modeOptions, verboseOptions, backupOptions);

            return await rootCommand.InvokeAsync(args);
        }

        private static void PatchAffinity(DirectoryInfo directoryInfo, bool verbose, bool keepOriginal)
        {
            AnsiConsole.MarkupLine("[blue]Starting Affinity patching process...[/]");

            var personaAssembly = FindAssembly("Serif.Interop.Persona.dll",directoryInfo);
            if (personaAssembly == null)
                throw new FileNotFoundException("Cannot find the required Affinity assembly (Serif.Interop.Persona.dll).");

            PatchAffinityAssembly(personaAssembly.FullName, verbose: verbose, keepOriginal: keepOriginal);
        }

        private static void PatchDxO(DirectoryInfo directoryInfo, bool verbose, bool keepOriginal)
        {
            AnsiConsole.MarkupLine("[blue]Starting DxO PhotoLab patching process...[/]");

            var activationAssembly = FindAssembly("DxO.PhotoLab.Activation.dll", directoryInfo);
            var activationInteropAssembly = FindAssembly("DxO.PhotoLab.Activation.Interop.dll", directoryInfo);

            if (activationAssembly == null || activationInteropAssembly == null)
                throw new FileNotFoundException("Cannot find the required DxO assemblies.");

            PatchDxOAssembly(activationAssembly.FullName, verbose: verbose, keepOriginal: keepOriginal);
            PatchDxOAssembly(activationInteropAssembly.FullName, verbose: verbose, keepOriginal: keepOriginal);
        }

        private static FileInfo? FindAssembly(string dllName, DirectoryInfo? directoryInfo)
        {
            // use the backup file if one exists
            var expectedPath = Path.Join(directoryInfo?.FullName, dllName);
            var expectedFi = new FileInfo(expectedPath);
            return expectedFi.Exists ? expectedFi : null;
        }

        private static void PatchAffinityAssembly(string targetFile, bool verbose, bool keepOriginal)
        {
            if (keepOriginal)
            {
                File.Copy(targetFile, targetFile + ".bak", overwrite: true);
                AnsiConsole.MarkupLine("[green]Backed up original Affinity assembly.[/]");
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

                        Patcher.PatchWithLdcRet(method.Body, 1);
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

                        Patcher.PatchWithLdcRet(method.Body, 0);
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

                    Patcher.PatchWithLdcRet(crashPolicy.Body, (int)CrashReportUploadPolicy.Never);
                    patchedList.Add(crashPolicy.FullName);
                }

                SaveAssembly(module, tempOutput, patchedList, "Affinity");
            }

            FinalizeAssembly(targetFile, tempOutput);
        }

        private static void PatchDxOAssembly(string targetFile, bool verbose, bool keepOriginal)
        {
            if (keepOriginal)
            {
                var backupPath = targetFile + ".bak";
                File.Copy(targetFile, backupPath, overwrite: true);
                AnsiConsole.MarkupLine($"[green]Backed up original DxO assembly as \"{backupPath}\".[/]");
            }

            var moduleContext = ModuleDef.CreateModuleContext();
            var tempOutput = Path.GetTempFileName();

            using (var module = ModuleDefMD.Load(targetFile, moduleContext))
            {
                var patchedList = new List<string>();
                var features = module.Types.Where(x => x.FullName.Contains("DxO.PhotoLab.Activation.Feature") || x.FullName.Contains("DxOActivation"));

                foreach (var feature in features)
                {
                    var methodsToPatchToTrue = feature?.Methods.Where(x => x.HasBody).Where(x =>
                        x.Name.EndsWith("IsValid") || x.Name.EndsWith("HasAnyLicense") ||
                        x.Name.EndsWith("IsActivated") || x.Name.EndsWith("IsElite"));

                    if (methodsToPatchToTrue != null)
                    {
                        foreach (var method in methodsToPatchToTrue)
                        {
                            Patcher.PatchWithLdcRetVerbose(method.FullName, method.Body, 1, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }

                    var methodsToPatchToFalse = feature?.Methods.Where(x => x.HasBody).Where(x =>
                        x.Name.EndsWith("IsExpired") || x.Name.EndsWith("IsDemo") ||
                        x.Name.EndsWith("IsTemporary") || x.Name == "Check");

                    if (methodsToPatchToFalse != null)
                    {
                        foreach (var method in methodsToPatchToFalse)
                        {
                            Patcher.PatchWithLdcRetVerbose(method.FullName, method.Body, 0, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }

                    var methodsToPatchToTwo = feature?.Methods.Where(x => x.HasBody).Where(x => x.Name.EndsWith("DemoType"));
                    if (methodsToPatchToTwo != null)
                    {
                        foreach (var method in methodsToPatchToTwo)
                        {
                            Patcher.PatchWithLdcRetVerbose(method.FullName, method.Body, 2, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }

                    var methodsToPatchToSpecifiedAmount = feature?.Methods.Where(x => x.HasBody).Where(x =>
                        x.Name.EndsWith("RemainingDays") || x.Name == "RemainingOfflineDays");

                    if (methodsToPatchToSpecifiedAmount != null)
                    {
                        foreach (var method in methodsToPatchToSpecifiedAmount)
                        {
                            Patcher.PatchWithLdcRetVerbose(method.FullName, method.Body, 99, verbose);
                            patchedList.Add(method.FullName);
                        }
                    }
                }

                SaveAssembly(module, tempOutput, patchedList, "DxO");
            }

            FinalizeAssembly(targetFile, tempOutput);
        }

        private static void SaveAssembly(ModuleDefMD module, string tempOutput, List<string> patchedList, string appName)
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Aesthetic)
                .Start($"Saving {appName} assembly...", _ =>
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

            var panel = new Panel(sb.ToString())
            {
                Padding = new Padding(1, 1)
            };
            panel.Header($"Patched ({appName})");
            AnsiConsole.Write(panel);
        }

        private static void FinalizeAssembly(string targetFile, string tempOutput)
        {
            if (targetFile.EndsWith(".bak")) targetFile = targetFile.Replace(".bak", "");

            File.Move(tempOutput, targetFile, overwrite: true);

            AnsiConsole.MarkupLine(
                $"[green]Assembly saved as {targetFile}[/] with file size [bold]{new FileInfo(targetFile).Length.Bytes().Humanize()}[/].");
        }
    }
}
