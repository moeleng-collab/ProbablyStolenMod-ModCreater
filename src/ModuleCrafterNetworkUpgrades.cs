#nullable disable
using System;

public static class ModuleCrafterNetworkUpgrades
{
    public const string SupplyId = "MODULE_CRAFTER_SUPPLY";
    public const string WorkshopId = "MODULE_CRAFTER_WORKSHOP";
    public const string BlackMarketId = "MODULE_CRAFTER_BLACK_MARKET";
    public const string PrecisionId = "MODULE_CRAFTER_PRECISION";
    public const string MasterId = "MODULE_CRAFTER_MASTER";
    public const string TabId = "ModuleCrafter";

    public static void Register()
    {
        NetworkUpgrade.RegisterTabLabel(TabId, () => ModuleCrafterLocalization.T("network_tab"));
        RegisterUpgrade(SupplyId, 300, null, () =>
        {
            Grant(ModuleCrafterIds.Chip(ModuleCrafterExpansionStock.RandomLegalSpecialKind()));
        });
        RegisterUpgrade(WorkshopId, 500, SupplyId, () =>
        {
            Grant(ModuleCrafterIds.Enhancer);
            Grant(ModuleCrafterIds.EnhanceCatalyst, 3);
        });
        RegisterUpgrade(BlackMarketId, 700, SupplyId, null);
        RegisterUpgrade(PrecisionId, 800, WorkshopId, () => Grant(ModuleCrafterIds.SafeguardClip));
        RegisterUpgrade(MasterId, 1200, PrecisionId + "|" + BlackMarketId, RefreshEnhancedValues, requireAll: false);
    }

    public static void Unregister()
    {
        NetworkUpgrade.RemoveModUpgrades(ModuleCrafterMod.ManifestId);
        NetworkUpgrade.RegisterTabLabel(TabId, null);
    }

    public static bool IsPrecisionUnlocked()
    {
        return PlayerStore.Instance != null && NetworkUpgrade.IsUnlocked(PrecisionId);
    }

    public static bool IsMasterUnlocked()
    {
        return PlayerStore.Instance != null && NetworkUpgrade.IsUnlocked(MasterId);
    }

    public static int PlaceLegalOrder()
    {
        if (PlayerStore.Instance == null || !NetworkUpgrade.IsUnlocked(SupplyId))
        {
            return 0;
        }

        int count = 2;
        if (NetworkUpgrade.IsUnlocked(WorkshopId))
        {
            count++;
        }

        if (IsMasterUnlocked())
        {
            count++;
        }

        int added = 0;
        for (int i = 0; i < count; i++)
        {
            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(
                ModuleCrafterIds.Chip(ModuleCrafterExpansionStock.RandomLegalSpecialKind()), isOwend: false, isStolen: false))
            {
                added++;
            }
        }

        return added;
    }

    public static int PlaceBlackMarketOrder()
    {
        if (PlayerStore.Instance == null || !NetworkUpgrade.IsUnlocked(BlackMarketId))
        {
            return 0;
        }

        ModuleCrafterChipKind[] pool =
        {
            ModuleCrafterChipKind.FarmNightmareCap,
            ModuleCrafterChipKind.NeuralCapped,
            ModuleCrafterChipKind.NeuralUncapped
        };
        ModuleCrafterChipKind kind = pool[RNG.GetRandomIntExculsive(0, pool.Length)];
        GameItem item = DirectoryMaster.Item(ModuleCrafterIds.Chip(kind));
        if (item == null)
        {
            return 0;
        }

        item.unitValue = (long)System.Math.Round(item.unitValue * 1.5, System.MidpointRounding.AwayFromZero);
        if (IsMasterUnlocked())
        {
            GameItem second = DirectoryMaster.Item(ModuleCrafterIds.Chip(pool[RNG.GetRandomIntExculsive(0, pool.Length)]));
            if (second != null)
            {
                second.unitValue = (long)System.Math.Round(second.unitValue * 1.5, System.MidpointRounding.AwayFromZero);
                PlayerStore.Instance.TryAddDirectSellingItemToTable(second, isOwend: false, isStolen: false);
            }
        }

        return PlayerStore.Instance.TryAddDirectSellingItemToTable(item, isOwend: false, isStolen: false) ? 1 : 0;
    }

    private static void RegisterUpgrade(string id, int cost, string prerequisite, Action onUnlock, bool requireAll = true)
    {
        NetworkUpgrade definition = new NetworkUpgrade(id)
        {
            subtitle = "Module Crafter",
            cost = cost,
            state = 1,
            source = ModuleCrafterMod.ManifestId,
            tab = TabId,
            iconKey = ModuleCrafterMod.ManifestId + ":" + id,
            titleProvider = () => ModuleCrafterLocalization.T("network_" + id + "_title"),
            descriptionProvider = () => ModuleCrafterLocalization.T("network_" + id + "_desc")
        };
        if (!string.IsNullOrEmpty(prerequisite))
        {
            if (requireAll)
            {
                definition.prerequisite.Add(prerequisite);
            }
            else
            {
                foreach (string item in prerequisite.Split('|'))
                {
                    definition.prerequisite.Add(item);
                }
            }
        }

        NetworkUpgrade.RegisterModUpgrade(definition, onUnlock);
    }

    private static void RefreshEnhancedValues()
    {
        // Unlocking the perk must also update modules enhanced before purchase.
        // RestoreAll recalculates from V0 and applies the master bonus only at +1
        // or higher, so unenhanced module prices remain unchanged.
        ModuleCrafterEnhanceState.RestoreAll();
        ModuleCrafterMod.LogMsg("Module Manufacturing Master unlocked; recalculated existing enhanced module values.");
    }

    private static void Grant(string itemId, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            GameItem item = DirectoryMaster.Item(itemId);
            if (item != null)
            {
                GeneralHelper.SetItemOwned(item, true);
                GraphUtils.EmporiumTryAdd(item);
            }
        }
    }
}
