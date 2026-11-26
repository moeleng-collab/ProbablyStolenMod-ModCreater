#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

internal static class ModuleCrafterSpecialEffectAdapters
{
    private const string CncBaseWorkloadTag = "MODULE_CRAFTER_CNC_BASE_WORKLOAD";
    internal static readonly HashSet<string> FurnaceDailyBonuses = new HashSet<string>(StringComparer.Ordinal);
    internal static bool addingFurnaceBonus;

    internal static ModuleCrafterSpecialEffectState Effect(GameItem item)
    {
        if (item != null && ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            return ModuleCrafterSpecialEffectRules.Evaluate(kind, ModuleCrafterEnhanceState.GetLevel(item));
        }

        return default;
    }

    internal static string DayKey(GameItem machine)
    {
        return machine != null ? machine.GetUniqueID() + ":" + (StoreStation.Instance != null ? StoreStation.Instance.dayCounter : 0) : "0";
    }

    internal static void Reset()
    {
        FurnaceDailyBonuses.Clear();
        addingFurnaceBonus = false;
    }
}

[HarmonyPatch(typeof(PlantHarvestContext), nameof(PlantHarvestContext.CountModule))]
internal static class ModuleCrafterHydroponicYieldPatch
{
    private static void Postfix(PlantHarvestContext __instance, string moduleId, ref int __result)
    {
        if (__instance?.modules == null)
        {
            return;
        }

        foreach (GameItem module in __instance.modules)
        {
            if (module.identifier != moduleId)
            {
                continue;
            }

            var effect = ModuleCrafterSpecialEffectAdapters.Effect(module);
            if (effect.YieldBonus > 0)
            {
                __result += effect.YieldBonus - 1;
            }
        }
    }
}

[HarmonyPatch(typeof(PlantHarvest))]
internal static class ModuleCrafterSmugglerHarvestPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(PlantHarvest), "Deliver");
    }

    private static void Postfix(PlantHarvestContext ctx, GameItem item)
    {
        if (ctx?.modules == null || item == null)
        {
            return;
        }

        foreach (GameItem module in ctx.modules)
        {
            if (module.identifier != "farm_module_smuggler")
            {
                continue;
            }

            var effect = ModuleCrafterSpecialEffectAdapters.Effect(module);
            if (effect.ContrabandScope == ModuleCrafterContrabandScope.AllOutputs
                || (effect.ContrabandScope == ModuleCrafterContrabandScope.HarvestedCrops && !item.IsTag("SEED_ITEM_TAG")))
            {
                ContrabandHelper.RemoveContrabandStatus(item);
            }
        }
    }
}

[HarmonyPatch(typeof(PlantList))]
internal static class ModuleCrafterSpecificHarvestPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("HarvestKotton")]
    private static void AddKottonFabric(PlantHarvestContext ctx)
    {
        foreach (GameItem module in ctx.modules)
        {
            if (module.identifier == "farm_module_kotton_loom")
            {
                int bonus = ModuleCrafterSpecialEffectAdapters.Effect(module).OutputBonus;
                if (bonus > 0)
                {
                    ctx.OutputById("kotton_fabric", bonus);
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("HarvestDreamCap")]
    private static void AddNightmareCaps(PlantHarvestContext ctx)
    {
        if (ctx.HasModule("farm_module_dream_cap"))
        {
            return;
        }

        foreach (GameItem module in ctx.modules)
        {
            if (module.identifier == "farm_module_nightmare_cap")
            {
                int bonus = ModuleCrafterSpecialEffectAdapters.Effect(module).OutputBonus;
                if (bonus > 0)
                {
                    ctx.OutputById("nightmare_cap", bonus);
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("HarvestFillerweed")]
    private static void AddFillerweed(PlantHarvestContext ctx)
    {
        foreach (GameItem module in ctx.modules)
        {
            if (module.identifier == "farm_module_fillerweed_production")
            {
                int bonus = ModuleCrafterSpecialEffectAdapters.Effect(module).YieldBonus - 4;
                if (bonus > 0)
                {
                    ctx.OutputById("fillerweed", bonus);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(WaterHelper), nameof(WaterHelper.AddLiquid))]
internal static class ModuleCrafterSolventRecoveryPatch
{
    private static void Prefix(GameItem waterContainer, ref int addAmount)
    {
        if (addAmount <= 0 || waterContainer?.parentInventory == null)
        {
            return;
        }

        GameItem machine = waterContainer.parentInventory.GetParentItem();
        int percent = 0;
        foreach (GameItem module in MachineHelper.GetModules(machine))
        {
            if (module.identifier == "evaporator_module_solvent_recovery")
            {
                percent = Math.Max(percent, ModuleCrafterSpecialEffectAdapters.Effect(module).RecoveryPercent);
            }
        }

        if (percent > 0)
        {
            addAmount = Math.Max(1, (addAmount * (100 + percent) + 99) / 100);
        }
    }
}

[HarmonyPatch(typeof(MachineDnaManipulator), nameof(MachineDnaManipulator.HasStabilizer))]
internal static class ModuleCrafterDnaSurvivalPatch
{
    private static void Postfix(GameItem machine, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        foreach (GameItem module in MachineHelper.GetModules(machine))
        {
            if (module.identifier == "dna_manipulator_module_precision"
                && ModuleCrafterSpecialEffectAdapters.Effect(module).KeepTarget)
            {
                __result = true;
                return;
            }
        }
    }
}

[HarmonyPatch(typeof(MachineDnaManipulator))]
internal static class ModuleCrafterDnaExtractionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("FinishExtract")]
    private static void AddFilteredUses(GameItem machine, GameItem seed, GameItem disk, bool success)
    {
        if (!success || disk == null || seed == null)
        {
            return;
        }

        PlantTrait trait = PlantGeneticsHelper.GetDiskTrait(disk);
        if (trait == null)
        {
            return;
        }

        foreach (GameItem module in MachineHelper.GetModules(machine))
        {
            if (module.identifier != "dna_manipulator_module_positive_filter"
                && module.identifier != "dna_manipulator_module_negative_filter")
            {
                continue;
            }

            int extra = ModuleCrafterSpecialEffectAdapters.Effect(module).ExtraTraitCount;
            if (extra > 0)
            {
                PlantGeneticsHelper.AddDiskUses(disk, PlantGeneticsHelper.ExtractionUses(seed, trait) * extra);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("EjectDisk")]
    private static bool KeepEnhancedDisk(GameItem machine)
    {
        foreach (GameItem module in MachineHelper.GetModules(machine))
        {
            var effect = ModuleCrafterSpecialEffectAdapters.Effect(module);
            if (effect.KeepDisk == ModuleCrafterDiskRetention.Always)
            {
                return false;
            }

            if (effect.KeepDisk == ModuleCrafterDiskRetention.FiftyPercent && RNG.Roll(50))
            {
                return false;
            }
        }

        return true;
    }
}

[HarmonyPatch(typeof(MachinePrinter), nameof(MachinePrinter.CountMeleeBatchModules))]
internal static class ModuleCrafterMeleeBatchPatch
{
    private static void Postfix(GameItem printer, ref int __result)
    {
        foreach (GameItem module in MachineHelper.GetModules(printer))
        {
            if (module.identifier == "printer_module_melee_batch")
            {
                __result += Math.Max(0, ModuleCrafterSpecialEffectAdapters.Effect(module).OutputBonus - 1);
            }
        }
    }
}

[HarmonyPatch(typeof(MachinePrinter))]
internal static class ModuleCrafterPrinterEnhancementPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("CaptureIngotQuality")]
    private static void ApplyPrinterModules(GameItem printer, GameItem chip, List<GameItem> tray)
    {
        int qualityBonus = 0;
        int cncReduction = 0;
        foreach (GameItem module in MachineHelper.GetModules(printer))
        {
            var effect = ModuleCrafterSpecialEffectAdapters.Effect(module);
            if (module.identifier == "printer_module_metal")
            {
                qualityBonus += effect.QualityBonus;
            }
            else if (module.identifier == "printer_module_cnc")
            {
                cncReduction = Math.Max(cncReduction, effect.WorkloadReductionPercent);
            }
        }

        if (qualityBonus != 0)
        {
            printer.ModifyTag("PRINTER_INGOT_QUALITY_BONUS_INT", delegate(TagState state)
            {
                state.Enable().SetInt(state.GetInt() + qualityBonus);
            });
        }

        if (chip == null || cncReduction <= 0
            || (!chip.IsTag("PRINTER_FRAME_CHIP_TAG") && !chip.IsTag("PRINTER_GUN_PART_CHIP_TAG")))
        {
            return;
        }

        if (!chip.IsTag("MODULE_CRAFTER_CNC_BASE_WORKLOAD"))
        {
            int original = chip.GetTagReadonly("PROGRESS_ITEM_TARGET_TAG").GetInt();
            chip.ModifyTag("MODULE_CRAFTER_CNC_BASE_WORKLOAD", delegate(TagState state)
            {
                state.Enable().SetInt(original);
            });
        }

        int baseWorkload = chip.GetTagReadonly("MODULE_CRAFTER_CNC_BASE_WORKLOAD").GetInt();
        int adjusted = Math.Max(1, (baseWorkload * (100 - cncReduction) + 99) / 100);
        chip.ModifyTag("PROGRESS_ITEM_TARGET_TAG", delegate(TagState state)
        {
            state.Enable().SetInt(adjusted);
        });
    }
}

[HarmonyPatch(typeof(MachineAlarm), nameof(MachineAlarm.Operate))]
internal static class ModuleCrafterAlarmReputationPatch
{
    private static bool Prefix(GameItem alarm)
    {
        if (alarm == null)
        {
            return false;
        }

        GameGridInventory moduleInv = MachineHelper.GetModuleInv(alarm);
        if (moduleInv?.childItems == null)
        {
            return false;
        }

        Dictionary<string, int> reputationByFaction = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (GameItem module in moduleInv.childItems)
        {
            if (module.identifier != "alarm_module_transmitter")
            {
                continue;
            }

            string faction = module.GetTagReadonly("TRANSMITTER_TARGET_TAG").GetString();
            int value = ModuleCrafterSpecialEffectAdapters.Effect(module).OutputBonus;
            reputationByFaction[faction] = Math.Max(reputationByFaction.TryGetValue(faction, out int current) ? current : 0, value);
        }

        foreach (KeyValuePair<string, int> pair in reputationByFaction)
        {
            switch (pair.Key)
            {
                case "FACTION_REVOLUTION":
                    StoreReputation.GetRevFaction().ModReputation(pair.Value);
                    break;
                case "FACTION_SECURITY":
                    StoreReputation.GetSecFaction().ModReputation(pair.Value);
                    break;
                case "FACTION_BLACK_MARKET":
                    StoreReputation.GetBMFaction().ModReputation(pair.Value);
                    break;
                case "FACTION_UPPER_LEVEL":
                    StoreReputation.GetULFaction().ModReputation(pair.Value);
                    break;
            }
        }

        return false;
    }
}

[HarmonyPatch(typeof(GraphUtils), nameof(GraphUtils.EmporiumTryAdd), new[] { typeof(GraphNodeStorage), typeof(GameItem) })]
internal static class ModuleCrafterFurnaceOutputPatch
{
    private static void Postfix(GraphNodeStorage currentNode, GameItem item)
    {
        if (ModuleCrafterSpecialEffectAdapters.addingFurnaceBonus
            || !(currentNode is GameInventory inventory) || item == null)
        {
            return;
        }

        GameItem machine = inventory.GetParentItem();
        if (machine == null || StoreStation.Instance == null)
        {
            return;
        }

        string key = ModuleCrafterSpecialEffectAdapters.DayKey(machine);
        if (item.identifier == "metal_ingot")
        {
            foreach (GameItem module in MachineHelper.GetModules(machine))
            {
                if (module.identifier != "furnace_module_blast")
                {
                    continue;
                }

                var effect = ModuleCrafterSpecialEffectAdapters.Effect(module);
                if (effect.RemovePurityPenalty)
                {
                    string purity = IngotPurityHelper.GetPurity(item);
                    if (!string.IsNullOrEmpty(purity))
                    {
                        IngotPurityHelper.SetPurity(item, IngotPurityHelper.GetPurityConst(purity, 1));
                    }
                }

                if (effect.OutputBonus >= 3 && ModuleCrafterSpecialEffectAdapters.FurnaceDailyBonuses.Add(key))
                {
                    GameItem extra = DirectoryMaster.Item("metal_ingot");
                    if (extra != null)
                    {
                        IngotPurityHelper.SetPurity(extra, IngotPurityHelper.GetPurity(item));
                        AddBonus(currentNode, extra);
                    }
                }
            }
        }
        else if (item.identifier == "scrap_metal")
        {
            foreach (GameItem module in MachineHelper.GetModules(machine))
            {
                if (module.identifier != "furnace_module_junk")
                {
                    continue;
                }

                int bonus = ModuleCrafterSpecialEffectAdapters.Effect(module).OutputBonus - 4;
                if (bonus > 0 && ModuleCrafterSpecialEffectAdapters.FurnaceDailyBonuses.Add(key))
                {
                    for (int i = 0; i < bonus; i++)
                    {
                        AddBonus(currentNode, DirectoryMaster.Item("scrap_metal"));
                    }
                }
            }
        }
    }

    private static void AddBonus(GraphNodeStorage target, GameItem item)
    {
        if (item == null)
        {
            return;
        }

        ModuleCrafterSpecialEffectAdapters.addingFurnaceBonus = true;
        try
        {
            if (GraphUtils.TryAcceptAll(target, item) > 0)
            {
                GraphUtils.EmporiumTryAdd(item);
            }
        }
        finally
        {
            ModuleCrafterSpecialEffectAdapters.addingFurnaceBonus = false;
        }
    }
}
