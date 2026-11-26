#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterDebug
{
    public const string EntrySource = "module_crafter";

    public static bool IsEnabled
    {
        get
        {
            string version = ModuleCrafterMod.ModVersion ?? "";
            if (version.StartsWith("0.", StringComparison.Ordinal))
            {
                return true;
            }

            try
            {
                if (ModHelper.ModFileExists(ModuleCrafterMod.ManifestId, "debug.on"))
                {
                    return true;
                }

                foreach (LoadedMod mod in ModLoader.Mods)
                {
                    if (mod?.Manifest == null || mod.Manifest.ID != ModuleCrafterMod.ManifestId)
                    {
                        continue;
                    }

                    string loaded = mod.Manifest.ModVersion ?? "";
                    if (loaded.StartsWith("0.", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }

    public static void Register()
    {
        bool enabled = IsEnabled;
        bool markerExists = ModHelper.ModFileExists(ModuleCrafterMod.ManifestId, "debug.on");
        ModuleCrafterMod.LogMsg("Debug registration: enabled=" + enabled
            + ", debug.on=" + markerExists
            + ", folder=" + (ModHelper.GetModFolder(ModuleCrafterMod.ManifestId) ?? "not resolved") + ".");
        if (!enabled)
        {
            return;
        }

        CustomUIDebugPanel.Remove(EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Grant kit", GrantKit, source: EntrySource, info: "Gives the machine, charged batteries, all recipe chips, refund items, materials and a quality module.");
        CustomUIDebugPanel.Add("Modding", "MC: Jump to day 21", () => SetDay(ModuleCrafterRules.AdvancedChipJacksonMinDay), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Jump to day 35", () => SetDay(ModuleCrafterRules.NeuralUncappedMinDay), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Skip night", SkipNight, source: EntrySource, info: "Runs nightly machine callbacks, then advances the day.");
        CustomUIDebugPanel.Add("Modding", "MC: Finish current job", FinishCurrentJobs, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Force next success", () => SetForceResult("success"), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Force next failure", () => SetForceResult("fail"), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Clear force result", () => SetForceResult(null), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Charge battery", () => ChargeBatteries(true), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Drain battery", () => ChargeBatteries(false), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Stock Jackson", () => LogForce(ModuleCrafterStock.ChannelJackson), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Queue retired merchant", () => ModuleCrafterMod.LogMsg(ModuleCrafterMerchant.QueueForDebug()), source: EntrySource, info: "Adds the retired module merchant to today's visitor queue.");
        CustomUIDebugPanel.Add("Modding", "MC: Stock thief", () => LogForce(ModuleCrafterStock.ChannelThief), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Stock underground", () => LogForce(ModuleCrafterStock.ChannelUnderground), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: Stock smuggler", () => LogForce(ModuleCrafterStock.ChannelSmuggler), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC: List stock channels", ListChannels, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Grant full kit", GrantFullKit, source: EntrySource, info: "All 28 chips and modules, components, themed materials, enhancer and native test machines.");
        CustomUIDebugPanel.Add("Modding", "MC3: Components x20", () => GiveUnits(ModuleCrafterExpansion.UniversalComponent, 20), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Grant native machines", GrantNativeMachines, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Modules +0", () => SetAllModuleLevels(0), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Modules +8", () => SetAllModuleLevels(8), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Modules +10", () => SetAllModuleLevels(10), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Modules +12", () => SetAllModuleLevels(12), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Unlock all upgrades", UnlockAllUpgrades, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Relock precision", RelockPrecision, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Recipe themed", () => SetAllRecipeVariants(ModuleCrafterRecipeVariant.Themed), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Recipe universal", () => SetAllRecipeVariants(ModuleCrafterRecipeVariant.Universal), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Run migration", RunMigration, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Stock all channels", StockAllChannels, source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Add 10000 cash", () => PlayerStore.Instance?.ModCash(10000), source: EntrySource);
        CustomUIDebugPanel.Add("Modding", "MC3: Dump state", DumpState, source: EntrySource);
        ModuleCrafterMod.LogMsg("Debug console registered on the vanilla F9 Modding panel.");
    }

    public static void Unregister()
    {
        CustomUIDebugPanel.Remove(EntrySource);
    }

    public static void GrantKit()
    {
        int added = 0;
        added += Give(ModuleCrafterIds.Machine);
        added += Give(ModuleCrafterIds.Enhancer);
        added += Give(ModuleCrafterIds.Battery);
        added += Give(ModuleCrafterIds.Battery);
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            added += Give(ModuleCrafterIds.Chip(kind));
        }

        for (int i = 0; i < 10; i++)
        {
            added += Give(ModuleCrafterIds.Electronic);
        }

        added += Give(ModuleCrafterIds.Corrupt);
        added += Give(ModuleCrafterIds.Corrupt);
        added += Give(ModuleCrafterIds.Ruined);
        added += Give(ModuleCrafterIds.Performance);
        added += Give(ModuleCrafterIds.Efficiency);
        added += Give(ModuleCrafterIds.Quality);
        added += Give(ModuleCrafterIds.Fineness);
        added += Give(ModuleCrafterIds.NeuralCapped);
        added += Give(ModuleCrafterIds.Quality);
        added += Give(ModuleCrafterIds.RefundTicket);
        added += Give(ModuleCrafterIds.RefundTicket);
        added += Give(ModuleCrafterIds.RefundCore);
        for (int i = 0; i < 12; i++)
        {
            added += Give(ModuleCrafterIds.EnhanceCatalyst);
            added += Give("scrap_metal");
        }

        for (int i = 0; i < 3; i++)
        {
            added += Give(ModuleCrafterIds.SafeguardClip);
        }

        ModuleCrafterMod.LogMsg("Debug kit granted (" + added + " items).");
    }

    public static void SetDay(int day)
    {
        if (StoreStation.Instance == null)
        {
            ModuleCrafterMod.LogMsg("No store station; cannot set day.");
            return;
        }

        StoreStation.Instance.dayCounter = Math.Max(1, day);
        ModuleCrafterMod.LogMsg("Day set to " + StoreStation.Instance.dayCounter + ".");
    }

    public static void SkipNight()
    {
        DebugPanel.SkipNightMachine();
        DebugPanel.SkipNightNormal();
        ModuleCrafterMod.LogMsg("Skipped one night.");
    }

    public static void FinishCurrentJobs()
    {
        int count = 0;
        foreach (GameItem machine in FindMachines())
        {
            if (MachineProgressHelper.GetMachineState(machine) != MachineProgressHelper.STATE_WORKING)
            {
                continue;
            }

            (int _, int target) = MachineProgressHelper.GetProgress(machine);
            machine.ModifyTag(MachineProgressHelper.MACHINE_PROGRESS_CURRENT_TAG, delegate(TagState progress)
            {
                progress.Enable().SetInt(target);
            });
            ModuleCrafterMachine.FinishJob(machine, skipNightCheck: true);
            count++;
        }

        ModuleCrafterMod.LogMsg("Finished " + count + " running job(s).");
    }

    public static void SetForceResult(string result)
    {
        int count = 0;
        foreach (GameItem machine in FindMachines())
        {
            ApplyForceResult(machine, result);
            count++;
        }

        if (PlayerStore.Instance != null)
        {
            foreach (GameItem machine in PlayerStore.Instance.FindAllItem())
            {
                if (machine != null && machine.IsTag(ModuleCrafterIds.EnhancerTag))
                {
                    ApplyForceResult(machine, result);
                    count++;
                }
            }
        }

        ModuleCrafterMod.LogMsg(count == 0
            ? "No Module Crafter or Module Enhancer found to apply force result."
            : "Force result set to " + (result ?? "cleared") + " on " + count + " machine(s).");
    }

    private static void ApplyForceResult(GameItem machine, string result)
    {
        if (string.IsNullOrEmpty(result))
        {
            machine.DisableTag(ModuleCrafterMachine.ForceResultTag);
            return;
        }

        machine.ModifyTag(ModuleCrafterMachine.ForceResultTag, delegate(TagState state)
        {
            state.Enable().SetString(result);
        });
    }

    public static void ChargeBatteries(bool fill)
    {
        int count = 0;
        foreach (GameItem machine in FindMachines())
        {
            if (fill)
            {
                MachineHelper.RechargeMachineBattery(machine, 10000, ignoreBlackout: true);
            }
            else
            {
                GameItem battery = MachineHelper.GetBatterySlot(machine)?.childItem;
                if (battery != null)
                {
                    int energy = PowerHelper.GetAvailableEnergyFromItem(battery, useBandwidth: false);
                    if (energy > 0)
                    {
                        PowerHelper.DrawPowerSource(battery, energy);
                    }
                }
            }

            count++;
        }

        ModuleCrafterMod.LogMsg((fill ? "Charged " : "Drained ") + count + " machine(s).");
    }

    private static void LogForce(string channel)
    {
        ModuleCrafterMod.LogMsg(ModuleCrafterStock.ForceChannel(channel));
    }

    private static void ListChannels()
    {
        ModuleCrafterMod.LogMsg("Stock channels: jackson (machine, refund ticket, refund core; visit and night shop each roll independently), retired module merchant (weekly after owning the crafter; chips, machine, tickets, materials), thief / underground (capped neural chip), smuggler (uncapped, day 35), crate / scrapyard (stand-in chip). Live hooks besides Jackson and the retired merchant are still debug-only.");
    }

    public static void GrantFullKit()
    {
        GrantKit();
        GrantNativeMachines();
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            Give(ModuleCrafterIds.OutputModule(kind));
        }

        GiveUnits(ModuleCrafterExpansion.UniversalComponent, 20);
        string[] materials =
        {
            "nutrifruit", "dream_cap", "kotton_fabric", "fillerweed", "hydroreed_filter",
            "common_chemical", "restricted_chemical", "flux_agent", "junk", "metal_ingot",
            "printer_plastic", "plant_dna_disk", "scrap_metal", "energy_credit"
        };
        foreach (string id in materials)
        {
            Give(id);
            Give(id);
        }
    }

    public static void GrantNativeMachines()
    {
        Give(PreBuiltItemHelper.CreateHydroponic());
        Give(PreBuiltItemHelper.CreateDnaManipulator());
        Give(PreBuiltItemHelper.CreatePrinter());
        Give(PreBuiltItemHelper.CreateFurnace());
        Give(PreBuiltItemHelper.CreateEvaporator(100));
        Give(MachineAlarm.Alarme());
        Give("blender");
        Give("heating_plate");
        Give("chem_finisher");
    }

    public static void SetAllModuleLevels(int level)
    {
        int count = 0;
        if (PlayerStore.Instance == null)
        {
            return;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (!ModuleCrafterEnhanceState.IsIntactSupportedModule(item))
            {
                continue;
            }

            ModuleCrafterEnhanceState.SetLevel(item, level);
            count++;
        }

        ModuleCrafterMod.LogMsg("Set " + count + " module(s) to +" + level + ".");
    }

    public static void UnlockAllUpgrades()
    {
        string[] ids =
        {
            ModuleCrafterNetworkUpgrades.SupplyId,
            ModuleCrafterNetworkUpgrades.WorkshopId,
            ModuleCrafterNetworkUpgrades.BlackMarketId,
            ModuleCrafterNetworkUpgrades.PrecisionId,
            ModuleCrafterNetworkUpgrades.MasterId
        };
        foreach (string id in ids)
        {
            if (PlayerStore.Instance != null && PlayerStore.Instance.networkUpgrade.ContainsKey(id)
                && !NetworkUpgrade.IsUnlocked(id))
            {
                NetworkUpgrade.GetUpgradeById(id).Unlock();
            }
        }

        ModuleCrafterMod.LogMsg("All Module Crafter network upgrades unlocked.");
    }

    public static void RelockPrecision()
    {
        if (PlayerStore.Instance != null && PlayerStore.Instance.networkUpgrade.ContainsKey(ModuleCrafterNetworkUpgrades.PrecisionId))
        {
            PlayerStore.Instance.networkUpgrade[ModuleCrafterNetworkUpgrades.PrecisionId].state = 1;
        }

        ModuleCrafterMod.LogMsg("Precision process locked again; specialized modules are capped at +8.");
    }

    public static void SetAllRecipeVariants(ModuleCrafterRecipeVariant variant)
    {
        int count = 0;
        foreach (GameItem machine in FindMachines())
        {
            machine.ModifyTag(ModuleCrafterMachine.VariantTag, delegate(TagState state)
            {
                state.Enable().SetString(variant.ToString());
            });
            count++;
        }

        ModuleCrafterMod.LogMsg("Set recipe variant to " + variant + " on " + count + " machine(s).");
    }

    public static void RunMigration()
    {
        ModHelper.SetModDataInt(ModuleCrafterMod.ManifestId, ModuleCrafterMigration.SaveVersionKey, 0);
        ModuleCrafterMigration.Run();
        ModuleCrafterMod.LogMsg("Migration rerun completed; current version is "
            + ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, ModuleCrafterMigration.SaveVersionKey, 0) + ".");
    }

    public static void StockAllChannels()
    {
        int added = 0;
        added += ModuleCrafterExpansionStock.PlaceJacksonStock();
        added += ModuleCrafterExpansionStock.PlaceRetiredStock();
        added += ModuleCrafterNetworkUpgrades.PlaceBlackMarketOrder();
        ModuleCrafterMod.LogMsg("Expansion stock placed: " + added + " item(s).");
    }

    public static void DumpState()
    {
        ModuleCrafterMod.LogMsg("Manufacturer=" + ModuleCrafterScenario.IsManufacturer()
            + ", precision=" + ModuleCrafterNetworkUpgrades.IsPrecisionUnlocked()
            + ", master=" + ModuleCrafterNetworkUpgrades.IsMasterUnlocked()
            + ", saveVersion=" + ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, ModuleCrafterMigration.SaveVersionKey, 0));
        if (PlayerStore.Instance == null)
        {
            return;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (!ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind))
            {
                continue;
            }

            int level = ModuleCrafterEnhanceState.GetLevel(item);
            ModuleCrafterMod.LogMsg(item.identifier + ": N=" + level
                + ", max=" + ModuleCrafterEnhanceRules.MaxLevelFor(kind, ModuleCrafterNetworkUpgrades.IsPrecisionUnlocked())
                + ", V0=" + ModuleCrafterEnhanceState.GetBaseValue(item)
                + ", value=" + item.unitValue);
        }
    }

    private static int GiveUnits(string id, int count)
    {
        GameItem item = DirectoryMaster.Item(id);
        if (item == null)
        {
            ModuleCrafterMod.LogMsg("Could not spawn " + id + ".");
            return 0;
        }

        item.SetUnitCount(count);
        return Give(item);
    }

    private static int Give(string id)
    {
        return Give(DirectoryMaster.Item(id));
    }

    private static int Give(GameItem item)
    {
        if (item == null)
        {
            return 0;
        }

        GeneralHelper.SetItemOwned(item, own: true);
        GraphUtils.EmporiumTryAdd(item);
        return 1;
    }

    private static IEnumerable<GameItem> FindMachines()
    {
        if (PlayerStore.Instance == null)
        {
            yield break;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (item != null && item.IsTag(ModuleCrafterIds.MachineTag))
            {
                yield return item;
            }
        }
    }
}
