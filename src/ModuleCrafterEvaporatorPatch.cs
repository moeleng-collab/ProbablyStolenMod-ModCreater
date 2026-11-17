#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

[HarmonyPatch]
internal static class ModuleCrafterEvaporatorPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (Type nested in typeof(MachineEvaporator).GetNestedTypes(BindingFlags.NonPublic))
        {
            foreach (MethodInfo method in nested.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name.Contains("<Evaporator>") && parameters.Length == 3
                    && parameters[0].ParameterType == typeof(GameItem)
                    && parameters[1].ParameterType == typeof(GameInventory)
                    && parameters[2].ParameterType == typeof(SlotMarker))
                {
                    yield return method;
                }
            }
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        FieldInfo machineField = original.DeclaringType.GetField("machine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (machineField == null)
        {
            return codes;
        }

        MethodInfo qualityCap = AccessTools.Method(typeof(ModuleCrafterEnhanceState), nameof(ModuleCrafterEnhanceState.EvaporatorQualityCap));
        MethodInfo intClamp = AccessTools.Method(typeof(Math), nameof(Math.Clamp), new[] { typeof(int), typeof(int), typeof(int) });
        List<CodeInstruction> output = new List<CodeInstruction>(codes.Count + 2);
        bool patched = false;
        foreach (CodeInstruction instruction in codes)
        {
            if (!patched && instruction.Calls(intClamp) && output.Count > 0 && IsInt32(output[output.Count - 1], 100))
            {
                output.RemoveAt(output.Count - 1);
                output.Add(new CodeInstruction(OpCodes.Ldarg_0));
                output.Add(new CodeInstruction(OpCodes.Ldfld, machineField));
                output.Add(new CodeInstruction(OpCodes.Call, qualityCap));
                patched = true;
            }

            output.Add(instruction);
        }

        if (patched)
        {
            ModuleCrafterMod.LogMsg("Evaporator quality cap patched in " + original.Name + ".");
        }

        return output;
    }

    private static bool IsInt32(CodeInstruction instruction, int value)
    {
        if (instruction.opcode == OpCodes.Ldc_I4)
        {
            return (int)instruction.operand == value;
        }

        if (instruction.opcode == OpCodes.Ldc_I4_S)
        {
            return Convert.ToInt32(instruction.operand) == value;
        }

        return false;
    }
}
