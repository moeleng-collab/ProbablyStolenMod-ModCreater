#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterMigration
{
    public const int TargetVersion = 3;
    public const string SaveVersionKey = "save_version";

    public static void Run()
    {
        if (PlayerStore.Instance == null)
        {
            return;
        }

        int version = ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, SaveVersionKey, 0);
        if (version >= TargetVersion)
        {
            return;
        }

        try
        {
            List<GameItem> items = PlayerStore.Instance.FindAllItem();
            foreach (GameItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                MigrateLegacyJob(item);
                MigrateSpecialModule(item);
                MigrateNeuralChip(item);
            }

            ModHelper.SetModDataInt(ModuleCrafterMod.ManifestId, SaveVersionKey, TargetVersion);
            ModuleCrafterMod.LogMsg("Save migration to version " + TargetVersion + " completed.");
        }
        catch (Exception ex)
        {
            // Every item migration is idempotent. Do not mark the save as complete;
            // the next load may safely resume after the underlying problem is fixed.
            ModuleCrafterMod.LogMsg("Save migration did not complete and will remain pending: " + ex);
        }
    }

    private static void MigrateLegacyJob(GameItem item)
    {
        if (!item.IsTag(ModuleCrafterIds.MachineTag) || item.IsTag(ModuleCrafterMachine.VariantTag))
        {
            return;
        }

        if (MachineProgressHelper.GetMachineState(item) == MachineProgressHelper.STATE_WORKING)
        {
            item.ModifyTag(ModuleCrafterMachine.VariantTag, delegate(TagState state)
            {
                state.Enable().SetString(ModuleCrafterRecipeVariant.Legacy.ToString());
            });
        }
    }

    private static void MigrateSpecialModule(GameItem item)
    {
        if (!ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind)
            || !ModuleCrafterExpansion.IsSpecialized(kind))
        {
            return;
        }

        ModuleCrafterEnhanceState.EnsureBaseSnapshot(item);
    }

    private static void MigrateNeuralChip(GameItem item)
    {
        if (!ModuleCrafterIds.TryGetChipKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            return;
        }

        if (kind == ModuleCrafterChipKind.NeuralCapped)
        {
            ContrabandHelper.InitContrabandItem(item, 3);
        }
        else if (kind == ModuleCrafterChipKind.NeuralUncapped)
        {
            ContrabandHelper.InitContrabandItem(item, 4);
        }
        else if (kind == ModuleCrafterChipKind.FarmNightmareCap)
        {
            ContrabandHelper.InitContrabandItem(item, 3);
        }
    }
}
