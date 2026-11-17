#nullable disable
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

public static class ModuleCrafterMerchant
{
    public const string Identifier = "retired_module_merchant";
    public const string SpriteKey = "retired_module_merchant";
    public const string PortraitRelativePath = "Textures/Items/retired_module_merchant.png";

    public static void OnGameLoaded()
    {
        try
        {
            TryRegisterPortrait();
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Retired module merchant portrait failed: " + ex);
        }
    }

    public static void OnGenerateCustomer(StoreClientManager manager)
    {
        try
        {
            if (manager == null)
            {
                return;
            }

            int day = StoreStation.Instance != null ? StoreStation.Instance.dayCounter : 0;
            if (!ModuleCrafterRules.ShouldVisitRetiredMerchant(day, ModuleCrafterStock.PlayerOwnsCrafter()))
            {
                return;
            }

            if (IsAlreadyQueued(manager))
            {
                return;
            }

            manager.AddClient(Create());
            ModuleCrafterMod.LogMsg("Queued retired module merchant on day " + day + ".");
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Retired module merchant visit hook failed: " + ex);
        }
    }

    public static string QueueForDebug()
    {
        if (PlayerStore.Instance == null || PlayerStore.Instance.storeClientManager == null)
        {
            return "no store";
        }

        StoreClientManager manager = PlayerStore.Instance.storeClientManager;
        if (IsAlreadyQueued(manager))
        {
            return "Retired module merchant is already in today's queue.";
        }

        manager.AddClient(Create());
        return "Retired module merchant queued for today. Talk to them to stock the counter.";
    }

    public static StoreClient Create()
    {
        TryRegisterPortrait();
        StoreClient storeClient = new StoreClient();
        storeClient.identifier = Identifier;
        storeClient.nonShootable = true;
        storeClient.displayName = ModuleCrafterLocalization.T("merchant_name");
        storeClient.spriteName = ResolveSpriteName();
        storeClient.clientFaction = "FACTION_LOWER_LEVEL";
        StoreClientFactionSetup.InitLowerLevelCitizen(storeClient);
        storeClient.clientIntent = StoreClient.ClientIntent.SELL;
        storeClient.clientItemFeatureBuying.Add("premiumBuy15");
        storeClient.mainDialogue.SetText(storeClient.displayName, LocHelper.GetLocalizedDialogue("dialog_retired_generic_1")).SetEndAction(delegate { PlaceStock(); });
        storeClient.AddBasicDialog();
        storeClient.CompleteClientCreation();
        return storeClient;
    }

    public static int PlaceStock()
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        int day = StoreStation.Instance != null ? StoreStation.Instance.dayCounter : 0;
        ModuleCrafterMerchantPityState pity = LoadPity();
        ModuleCrafterMerchantOffer offer = ModuleCrafterRules.RollRetiredMerchantOffer(day, pity, NextExclusive, out ModuleCrafterMerchantPityState nextPity);
        SavePity(nextPity);
        ModuleCrafterMod.LogMsg(
            "Retired module merchant pity: capped "
            + ModuleCrafterRules.RetiredMerchantPityChance(pity.NeuralCappedMisses) + "% -> "
            + nextPity.NeuralCappedMisses + " misses, uncapped "
            + ModuleCrafterRules.RetiredMerchantPityChance(pity.NeuralUncappedMisses) + "% -> "
            + nextPity.NeuralUncappedMisses + " misses, core "
            + ModuleCrafterRules.RetiredMerchantPityChance(pity.RefundCoreMisses) + "% -> "
            + nextPity.RefundCoreMisses + " misses.");
        int placed = PlaceOffer(offer);
        placed += ModuleCrafterEnhanceSources.PlaceEnhancementStock(day);
        return placed;
    }

    public static int PlaceOffer(ModuleCrafterMerchantOffer offer)
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        int added = 0;
        added += AddCopies(ModuleCrafterIds.Machine, offer.MachineCount);
        added += AddCopies(ModuleCrafterIds.Enhancer, offer.EnhancerCount);
        if (offer.PerformanceChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Performance), 1);
        }

        if (offer.EfficiencyChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Efficiency), 1);
        }

        if (offer.QualityChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Quality), 1);
        }

        if (offer.OverclockChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Overclock), 1);
        }

        if (offer.EcoChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Eco), 1);
        }

        if (offer.FinenessChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Fineness), 1);
        }

        if (offer.StreamliningChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Streamlining), 1);
        }

        if (offer.NeuralCappedChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralCapped), 1);
        }

        if (offer.NeuralUncappedChip)
        {
            added += AddCopies(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralUncapped), 1);
        }

        if (offer.RefundCore)
        {
            added += AddCopies(ModuleCrafterIds.RefundCore, 1);
        }

        added += AddCopies(ModuleCrafterIds.RefundTicket, offer.TicketCount);
        added += AddCopies(ModuleCrafterIds.Electronic, offer.ElectronicCount);
        added += AddCopies(ModuleCrafterIds.Corrupt, offer.CorruptHousingCount);
        ModuleCrafterMod.LogMsg("Retired module merchant placed " + added + " item(s).");
        return added;
    }

    private static int AddCopies(string itemId, int count)
    {
        int added = 0;
        for (int i = 0; i < count; i++)
        {
            PlayerStore.Instance.AddDirectSellingItemToTable(itemId, isOwend: false, isStolen: false);
            added++;
        }

        return added;
    }

    private static bool IsAlreadyQueued(StoreClientManager manager)
    {
        if (manager?.clientStack == null)
        {
            return false;
        }

        foreach (StoreClient client in manager.clientStack)
        {
            if (client != null && client.identifier == Identifier)
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveSpriteName()
    {
        if (TryRegisterPortrait())
        {
            return SpriteKey;
        }

        return SpriteDict.GetRandomLowerLevelSpriteName();
    }

    internal static bool TryRegisterPortrait()
    {
        if (SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
        {
            return false;
        }

        if (SpriteDict.Instance.spriteDictionary.ContainsKey(SpriteKey)
            && SpriteDict.Instance.spriteDictionary[SpriteKey] != null)
        {
            return true;
        }

        Sprite sprite = FindLoadedPortraitSprite();
        if (sprite == null)
        {
            return false;
        }

        SpriteDict.Instance.spriteDictionary[SpriteKey] = sprite;
        ModuleCrafterMod.LogMsg("Registered retired module merchant portrait.");
        return true;
    }

    private static Sprite FindLoadedPortraitSprite()
    {
        if (ModLoader.Mods == null)
        {
            return null;
        }

        foreach (LoadedMod mod in ModLoader.Mods)
        {
            if (mod != null
                && mod.Manifest != null
                && mod.Manifest.ID == ModuleCrafterMod.ManifestId
                && mod.Sprites != null
                && mod.Sprites.TryGetValue(SpriteKey, out Sprite sprite)
                && sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    private const string PityCappedKey = "retired_merchant_pity_neural_capped";
    private const string PityUncappedKey = "retired_merchant_pity_neural_uncapped";
    private const string PityCoreKey = "retired_merchant_pity_core";

    private static ModuleCrafterMerchantPityState LoadPity()
    {
        return new ModuleCrafterMerchantPityState
        {
            NeuralCappedMisses = Math.Max(0, ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, PityCappedKey, 0)),
            NeuralUncappedMisses = Math.Max(0, ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, PityUncappedKey, 0)),
            RefundCoreMisses = Math.Max(0, ModHelper.GetModDataInt(ModuleCrafterMod.ManifestId, PityCoreKey, 0))
        };
    }

    private static void SavePity(ModuleCrafterMerchantPityState pity)
    {
        ModHelper.SetModDataInt(ModuleCrafterMod.ManifestId, PityCappedKey, Math.Max(0, pity.NeuralCappedMisses));
        ModHelper.SetModDataInt(ModuleCrafterMod.ManifestId, PityUncappedKey, Math.Max(0, pity.NeuralUncappedMisses));
        ModHelper.SetModDataInt(ModuleCrafterMod.ManifestId, PityCoreKey, Math.Max(0, pity.RefundCoreMisses));
    }

    private static int NextExclusive(int maxExclusive)
    {
        return RNG.GetRandomIntExculsive(0, maxExclusive);
    }
}


[HarmonyPatch(typeof(SpriteDict), "InitializeDictionary")]
internal static class ModuleCrafterSpriteDictPatch
{
    private static void Postfix()
    {
        try
        {
            ModuleCrafterMerchant.TryRegisterPortrait();
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Retired module merchant portrait failed: " + ex);
        }
    }
}
