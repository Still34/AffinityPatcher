using dnlib.DotNet.Emit;
using Spectre.Console;

namespace AffinityPatcher.Utils;

public static class Patcher
{
    public static void PatchWithLdcRet(CilBody cilBody, int ldcValue)
    {
        cilBody.Instructions.Clear();
        cilBody.ExceptionHandlers.Clear();
        cilBody.Variables.Clear();
        cilBody.Instructions.Add(Instruction.CreateLdcI4(ldcValue));
        cilBody.Instructions.Add(Instruction.Create(OpCodes.Ret));
    }

    public static void PatchWithLdcRetVerbose(string methodFullName, CilBody cilBody, int ldcValue, bool verbose = false)
    {
        if (verbose)
            AnsiConsole.MarkupLine(
                $"Located [grey]{methodFullName}[/], patching with [grey]\"return {ldcValue};\"[/].");

        PatchWithLdcRet(cilBody, ldcValue);
    }
}