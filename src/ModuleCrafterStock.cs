#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterStock
{
    public const string ChannelJackson = "jackson";
    public const string ChannelThief = "thief";
    public const string ChannelUnderground = "underground";
    public const string ChannelSmuggler = "smuggler";
    public const string ChannelCrate = "crate";
    public const string ChannelScrapyard = "scrapyard";

    public static void OnPlaceInventorInventory(List<GameItem> ownedItems)
    {
        try
        {
            PlaceJacksonStock();
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Jackson stock hook failed: " + ex);
        }
    }

    public static int PlaceJacksonStock()
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        int day = StoreStation.Instance != null ? StoreStation.Instance.dayCounter : 0;
        ModuleCrafterJacksonDayOffer offer = ModuleCrafterRules.RollJacksonDayOffer(
            day,
            PlayerStore.Instance.IsPlayerHaveMachine(),
            PlayerOwnsCrafter(),
            NextExclusive,
            PlayerOwnsEnhancer());

        int added = 0;
        foreach (ModuleCrafterJacksonStockKey key in (ModuleCrafterJacksonStockKey[])Enum.GetValues(typeof(ModuleCrafterJacksonStockKey)))
        {
            if (!offer.IsListed(key))
            {
                continue;
            }

            PlayerStore.Instance.AddDirectSellingItemToTable(
                ModuleCrafterRules.JacksonStockItemId(key),
                isOwend: false,
                isStolen: false);
            added++;
            ModuleCrafterMod.LogMsg("Jackson listed " + ModuleCrafterRules.JacksonStockItemId(key) + ".");
        }

        return added;
    }

    public static bool PlayerOwnsCrafter()
    {
        return PlayerOwns(ModuleCrafterIds.Machine);
    }

    public static bool PlayerOwnsEnhancer()
    {
        return PlayerOwns(ModuleCrafterIds.Enhancer);
    }

    private static bool PlayerOwns(string itemId)
    {
        if (PlayerStore.Instance == null)
        {
            return false;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (item != null && item.identifier == itemId && GeneralHelper.IsItemOwned(item))
            {
                return true;
            }
        }

        return false;
    }

    public static string ForceChannel(string channel)
    {
        if (PlayerStore.Instance == null)
        {
            return "no store";
        }

        switch ((channel ?? "").Trim().ToLowerInvariant())
        {
            case ChannelJackson:
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Machine, isOwend: false, isStolen: false);
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Enhancer, isOwend: false, isStolen: false);
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.RefundTicket, isOwend: false, isStolen: false);
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.RefundCore, isOwend: false, isStolen: false);
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.EnhanceCatalyst, isOwend: false, isStolen: false);
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.SafeguardClip, isOwend: false, isStolen: false);
                return "Jackson: machines, refund items, enhancement catalyst, and stabilizing clip on the counter.";
            case ChannelThief:
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralCapped), isOwend: false, isStolen: true);
                return "Thief: capped neural chip on the counter.";
            case ChannelUnderground:
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralCapped), isOwend: false, isStolen: true);
                return "Underground: capped neural chip on the counter.";
            case ChannelSmuggler:
                if (StoreStation.Instance != null
                    && StoreStation.Instance.dayCounter < ModuleCrafterRules.NeuralUncappedMinDay)
                {
                    StoreStation.Instance.dayCounter = ModuleCrafterRules.NeuralUncappedMinDay;
                }

                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralUncapped), isOwend: false, isStolen: false);
                return "Hypertech smuggler: uncapped neural chip on the counter (day gate applied).";
            case ChannelCrate:
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Performance), isOwend: false, isStolen: false);
                return "Crate stand-in: performance chip on the counter.";
            case ChannelScrapyard:
                PlayerStore.Instance.AddDirectSellingItemToTable(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Efficiency), isOwend: false, isStolen: false);
                return "Scrapyard stand-in: efficiency chip on the counter.";
            default:
                return "Unknown channel: " + channel;
        }
    }

    private static int NextExclusive(int maxExclusive)
    {
        return RNG.GetRandomIntExculsive(0, maxExclusive);
    }
}
