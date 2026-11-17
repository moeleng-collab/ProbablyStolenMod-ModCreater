#nullable disable
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

public static class ModuleCrafterEnhanceSources
{
    private const string RetiredOfferKey = "retired_merchant_enhance_day_offer_v2";
    private static readonly HashSet<Dialogue> WrappedDialogues = new HashSet<Dialogue>();
    internal static bool IsActive;

    public static int PlaceEnhancementStock(int day)
    {
        if (PlayerStore.Instance == null)
        {
            return 0;
        }

        ModuleCrafterEnhanceDayOffer? current = null;
        string saved = ModHelper.GetModData(ModuleCrafterMod.ManifestId, RetiredOfferKey);
        if (ModuleCrafterEnhanceDayOffer.TryParse(saved, out ModuleCrafterEnhanceDayOffer parsed))
        {
            current = parsed;
        }

        ModuleCrafterEnhanceDayOffer offer = ModuleCrafterEnhanceRules.EnsureRetiredDayOffer(current, day, NextExclusive);
        ModHelper.SetModData(ModuleCrafterMod.ManifestId, RetiredOfferKey, offer.Encode());
        int added = 0;
        added += AddToCounter(ModuleCrafterIds.EnhanceCatalyst, offer.CatalystCount, stolen: false);
        added += AddToCounter(ModuleCrafterIds.SafeguardClip, offer.SafeguardClipCount, stolen: false);
        ModuleCrafterMod.LogMsg("Retired merchant enhancement stock on day " + day + ": " + offer.CatalystCount + " catalyst(s), " + offer.SafeguardClipCount + " clip(s); placed " + added + ".");
        return added;
    }

    public static void OnGenerateCustomerLate(StoreClientManager manager)
    {
        if (!IsActive || manager?.clientStack == null)
        {
            return;
        }

        foreach (StoreClient client in manager.clientStack)
        {
            if (client == null)
            {
                continue;
            }

            string identifier = client.identifier;
            int chance = ModuleCrafterEnhanceRules.AdditionalSellerChance(identifier, client.wholesaleDiscount == -20);
            if (chance <= 0)
            {
                continue;
            }

            try
            {
                WrapFinalDialogue(client, chance);
            }
            catch (Exception ex)
            {
                ModuleCrafterMod.LogMsg("Enhancement seller hook failed for " + identifier + ": " + ex.Message);
            }
        }
    }

    private static void WrapFinalDialogue(StoreClient client, int chance)
    {
        Dialogue dialogue = client.mainDialogue;
        if (dialogue == null)
        {
            return;
        }

        HashSet<Dialogue> visited = new HashSet<Dialogue>();
        while (dialogue.nextDialogue != null && visited.Add(dialogue))
        {
            dialogue = dialogue.nextDialogue;
        }

        if (!WrappedDialogues.Add(dialogue))
        {
            return;
        }

        string identifier = client.identifier;
        bool stolen = ModuleCrafterEnhanceRules.SellerItemsAreStolen(identifier);
        Action original = dialogue.endAction;
        dialogue.endAction = delegate
        {
            original?.Invoke();
            if (!IsActive || PlayerStore.Instance == null || !ModuleCrafterRules.RollStock(chance, NextExclusive))
            {
                return;
            }

            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(ModuleCrafterIds.EnhanceCatalyst, isOwend: false, isStolen: stolen))
            {
                ModuleCrafterMod.LogMsg("Enhancement catalyst added by seller " + identifier + ".");
            }
        };
    }

    private static int AddToCounter(string itemId, int count, bool stolen)
    {
        int added = 0;
        for (int i = 0; i < Math.Max(0, count); i++)
        {
            if (PlayerStore.Instance.TryAddDirectSellingItemToTable(itemId, isOwend: false, isStolen: stolen))
            {
                added++;
            }
        }

        return added;
    }

    private static int NextExclusive(int exclusive)
    {
        return RNG.GetRandomIntExculsive(0, exclusive);
    }
}

[HarmonyPatch(typeof(ScavHelper))]
internal static class ModuleCrafterEnhanceScavengePatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(ScavHelper), nameof(ScavHelper.GetRandomScavengedItem), new[] { typeof(GameItem).MakeByRefType() });
    }

    private static void Postfix(List<GameItem> __result)
    {
        if (!ModuleCrafterEnhanceSources.IsActive || __result == null
            || !ModuleCrafterRules.RollStock(ModuleCrafterEnhanceRules.ScavengeBonusChance, NextExclusive))
        {
            return;
        }

        GameItem catalyst = DirectoryMaster.Item(ModuleCrafterIds.EnhanceCatalyst);
        if (catalyst != null)
        {
            __result.Add(catalyst);
            ModuleCrafterMod.LogMsg("Enhancement catalyst added to scavenging results.");
        }
    }

    private static int NextExclusive(int exclusive)
    {
        return RNG.GetRandomIntExculsive(0, exclusive);
    }
}
