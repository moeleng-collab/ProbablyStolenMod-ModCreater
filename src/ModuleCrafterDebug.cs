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
        if (!IsEnabled)
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

    private static int Give(string id)
    {
        GameItem item = DirectoryMaster.Item(id);
        if (item == null)
        {
            ModuleCrafterMod.LogMsg("Could not spawn " + id + ".");
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
