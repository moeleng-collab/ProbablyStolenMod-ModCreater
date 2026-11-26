#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterEnhancement
{
    public static void RegisterItems(ModItemDirectory directory)
    {
        directory.Add(ModuleCrafterIds.EnhanceCatalyst, () => CreateSupply(ModuleCrafterIds.EnhanceCatalyst));
        directory.Add(ModuleCrafterIds.SafeguardClip, () => CreateSupply(ModuleCrafterIds.SafeguardClip));
    }

    public static void OnTooltip(RichTextBuilder builder, GameItem item)
    {
        if (item == null)
        {
            return;
        }

        if (item.identifier == ModuleCrafterIds.EnhanceCatalyst || item.identifier == ModuleCrafterIds.SafeguardClip)
        {
            builder.AddLine(ModuleCrafterLocalization.T(item.identifier == ModuleCrafterIds.EnhanceCatalyst ? "catalyst_desc" : "clip_desc"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
            return;
        }

        if (ModuleCrafterEnhanceState.HasEnhancementState(item) && ModuleCrafterEnhanceState.IsIntactSupportedModule(item))
        {
            int level = ModuleCrafterEnhanceState.GetLevel(item);
            builder.AddLine(ModuleCrafterLocalization.T("tooltip_enhance_level", level), ignoreEmpty: true, RenderHandler.ColorPalette.LightGreen);
            if (ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind))
            {
                if (ModuleCrafterExpansion.IsSpecialized(kind))
                {
                    builder.AddLine(ModuleCrafterLocalization.T(
                        "tooltip_special_effect",
                        ModuleCrafterLocalization.SpecialEffectSummary(kind, level)), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
                }

                if (level < ModuleCrafterEnhanceRules.MaxLevelFor(kind, ModuleCrafterNetworkUpgrades.IsPrecisionUnlocked()))
                {
                    builder.AddLine(ModuleCrafterLocalization.T(
                        "tooltip_enhance_next",
                        ModuleCrafterScenario.ApplySuccessBonus(ModuleCrafterEnhanceRules.SuccessPercent(level)),
                        ModuleCrafterEnhanceRules.GoldCost(kind, level),
                        ModuleCrafterEnhanceRules.RequiredCatalysts(level)), ignoreEmpty: true, RenderHandler.ColorPalette.LightYellow);
                }
            }
        }
    }

    public static void RefreshPreview(MachineryText preview, GameGridInventory materials)
    {
        if (preview == null)
        {
            return;
        }

        GameItem item = FindTarget(materials);
        if (item == null)
        {
            preview.SetText(ModuleCrafterLocalization.T("enhance_empty") + ExchangeSuffix(materials));
            return;
        }

        if (!ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            preview.SetText(ModuleCrafterLocalization.T("enhance_invalid"));
            return;
        }

        int level = ModuleCrafterEnhanceState.GetLevel(item);
        int maxLevel = ModuleCrafterEnhanceRules.MaxLevelFor(kind, ModuleCrafterNetworkUpgrades.IsPrecisionUnlocked());
        if (level >= maxLevel)
        {
            string key = maxLevel < ModuleCrafterEnhanceRules.MaxLevel ? "enhance_locked" : "enhance_max";
            preview.SetText(ModuleCrafterLocalization.T(key) + StatPreview(item, level, level));
            return;
        }

        int cost = ModuleCrafterEnhanceRules.GoldCost(kind, level);
        int catalysts = ModuleCrafterEnhanceRules.RequiredCatalysts(level);
        string text = ModuleCrafterLocalization.T(
            "enhance_ready",
            level,
            level + 1,
            ModuleCrafterScenario.ApplySuccessBonus(ModuleCrafterEnhanceRules.SuccessPercent(level)),
            cost,
            catalysts,
            CountUnits(materials, ModuleCrafterIds.SafeguardClip))
            + StatPreview(item, level, level + 1)
            + SpecialPreview(kind, level + 1)
            + BreakPreview(item, level)
            + FailurePreview(level, CountUnits(materials, ModuleCrafterIds.SafeguardClip) > 0);

        if (PlayerStore.Instance == null || !PlayerStore.Instance.CanPay(cost))
        {
            text += ModuleCrafterLocalization.T("enhance_need_gold", cost);
        }
        else if (CountUnits(materials, ModuleCrafterIds.EnhanceCatalyst) < catalysts)
        {
            text += ModuleCrafterLocalization.T("enhance_need_catalyst", catalysts);
        }

        preview.SetText(text + ExchangeSuffix(materials));
    }

    public static bool TryEnhance(GameItem machine, MachineryText preview, GameGridInventory materials)
    {
        GameItem item = FindTarget(materials);
        if (machine == null || item == null || item.parentInventory != materials
            || !ModuleCrafterEnhanceState.TryGetKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            RefreshPreview(preview, materials);
            return false;
        }

        int from = ModuleCrafterEnhanceState.GetLevel(item);
        if (!ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, false, from, kind, ModuleCrafterNetworkUpgrades.IsPrecisionUnlocked()))
        {
            RefreshPreview(preview, materials);
            return false;
        }

        int cost = ModuleCrafterEnhanceRules.GoldCost(kind, from);
        int catalystCost = ModuleCrafterEnhanceRules.RequiredCatalysts(from);
        if (PlayerStore.Instance == null || !PlayerStore.Instance.CanPay(cost)
            || CountUnits(materials, ModuleCrafterIds.EnhanceCatalyst) < catalystCost)
        {
            RefreshPreview(preview, materials);
            return false;
        }

        // Prepare B/N before the transaction, then revalidate the exact grid reference.
        ModuleCrafterEnhanceState.EnsureBaseSnapshot(item);
        if (item.parentInventory != materials || FindTarget(materials) != item)
        {
            RefreshPreview(preview, materials);
            return false;
        }

        // The perk waives payment only after normal affordability checks; it never bypasses them.
        bool tinkererFree = ModuleCrafterEnhanceRules.RollTinkererFreeEnhance(StartingPerk.IsTinkerer(), NextExclusive);
        if (!tinkererFree)
        {
            PlayerStore.Instance.PayMoney(cost, false);
            ConsumeUnits(materials, ModuleCrafterIds.EnhanceCatalyst, catalystCost);
        }

        bool success;
        if (machine.IsTag(ModuleCrafterMachine.ForceResultTag))
        {
            success = string.Equals(machine.GetTagReadonly(ModuleCrafterMachine.ForceResultTag).GetString(), "success", StringComparison.OrdinalIgnoreCase);
            machine.DisableTag(ModuleCrafterMachine.ForceResultTag);
        }
        else
        {
            success = ModuleCrafterRules.RollSuccess(ModuleCrafterScenario.ApplySuccessBonus(ModuleCrafterEnhanceRules.SuccessPercent(from)), NextExclusive);
        }

        bool hasClip = CountUnits(materials, ModuleCrafterIds.SafeguardClip) > 0;
        ModuleCrafterEnhanceResult result = ModuleCrafterEnhanceRules.Resolve(success, from, hasClip);
        GameItem visualTarget = item;
        if (result.Kind == ModuleCrafterEnhanceResultKind.Ruined)
        {
            visualTarget = ReplaceWithRuined(item);
            GiveUnits(materials, ModuleCrafterIds.EnhanceCatalyst,
                ModuleCrafterEnhanceRules.CatalystRefund(result, tinkererFree));
        }
        else
        {
            ConsumeUnits(materials, ModuleCrafterIds.SafeguardClip,
                ModuleCrafterEnhanceRules.SafeguardClipConsumption(result, tinkererFree));
            ModuleCrafterEnhanceState.SetLevel(item, result.ToLevel);
        }

        ModuleCrafterEnhanceVisuals.Play(machine, visualTarget, result);
        ModuleCrafterEnhanceAudio.Play(result.Kind);
        ModuleCrafterMod.LogMsg("Enhance " + kind + " from " + from + " => " + result.Kind + " to " + result.ToLevel
            + (tinkererFree ? "; Tinkerer waived gold and material consumption." : "."));
        if (preview != null)
        {
            preview.SetText(ModuleCrafterLocalization.T(ResultKey(result.Kind), from, result.ToLevel) + ExchangeSuffix(materials));
        }

        return true;
    }

    public static bool TryExchangeScrap(MachineryText preview, GameGridInventory materials)
    {
        if (materials == null || CountUnits(materials, ModuleCrafterIds.Scrap) < ModuleCrafterEnhanceRules.ScrapPerCatalyst)
        {
            if (preview != null)
            {
                preview.SetText(ModuleCrafterLocalization.T("exchange_missing") + ExchangeSuffix(materials));
            }

            return false;
        }

        GameItem catalyst = DirectoryMaster.Item(ModuleCrafterIds.EnhanceCatalyst);
        if (catalyst == null)
        {
            return false;
        }

        GeneralHelper.SetItemOwned(catalyst, own: true);
        if (!GraphUtils.CanAccept(materials, catalyst))
        {
            catalyst.Destroy();
            if (preview != null)
            {
                preview.SetText(ModuleCrafterLocalization.T("exchange_no_space") + ExchangeSuffix(materials));
            }

            return false;
        }

        ConsumeUnits(materials, ModuleCrafterIds.Scrap, ModuleCrafterEnhanceRules.ScrapPerCatalyst);
        if (GraphUtils.TryAcceptAll(materials, catalyst) > 0 && catalyst.parentInventory == null)
        {
            // Revalidation should make this rare; never destroy an unplaced refund/output.
            GraphUtils.EmporiumTryAdd(catalyst);
            ModuleCrafterMod.LogMsg("Enhancer exchange output overflowed to the store inventory.");
        }

        if (preview != null)
        {
            preview.SetText(ModuleCrafterLocalization.T("exchange_done") + ExchangeSuffix(materials));
        }

        return true;
    }

    public static GameItem FindTarget(GameInventory materials)
    {
        if (materials == null)
        {
            return null;
        }

        foreach (GameItem item in SlotItems(materials))
        {
            if (ModuleCrafterEnhanceState.IsIntactSupportedModule(item))
            {
                return item;
            }
        }

        return null;
    }

    private static int CountUnits(GameInventory materials, string itemId)
    {
        int count = 0;
        foreach (GameItem item in SlotItems(materials))
        {
            if (item.identifier == itemId)
            {
                count += Math.Max(0, item.unitCount);
            }
        }

        return count;
    }

    private static void ConsumeUnits(GameInventory materials, string itemId, int amount)
    {
        int remaining = Math.Max(0, amount);
        if (remaining == 0)
        {
            return;
        }

        foreach (GameItem item in new List<GameItem>(SlotItems(materials)))
        {
            if (remaining <= 0)
            {
                break;
            }

            if (item.identifier != itemId)
            {
                continue;
            }

            int take = Math.Min(remaining, Math.Max(0, item.unitCount));
            if (take <= 0)
            {
                continue;
            }

            remaining -= take;
            if (take >= item.unitCount)
            {
                item.Destroy();
            }
            else
            {
                item.SetUnitCount(item.unitCount - take);
            }
        }

        if (remaining > 0)
        {
            throw new InvalidOperationException("Validated enhancer material consumption failed for " + itemId + ".");
        }
    }

    private static void GiveUnits(GameGridInventory materials, string itemId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameItem item = DirectoryMaster.Item(itemId);
            if (item == null)
            {
                continue;
            }

            GeneralHelper.SetItemOwned(item, own: true);
            if (materials == null || GraphUtils.TryAcceptAll(materials, item) > 0)
            {
                if (item.parentInventory == null)
                {
                    GraphUtils.EmporiumTryAdd(item);
                }
            }
        }
    }

    private static GameItem CreateSupply(string id)
    {
        bool catalyst = id == ModuleCrafterIds.EnhanceCatalyst;
        GameItem item = ItemDirectory.CreateEmptyItem(id)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, id)
            .SetName(ModuleCrafterLocalization.T(catalyst ? "catalyst_name" : "clip_name"));
        if (!catalyst)
        {
            // 3x2 T shape: three cells across the top, one centered below.
            item.SetShape(new GridShapeBuilder().SetData("111010", 3));
        }

        item.shortDescription = ModuleCrafterLocalization.T(catalyst ? "catalyst_desc" : "clip_desc");
        item.flavorText = ModuleCrafterLocalization.T(catalyst ? "catalyst_flavor" : "clip_flavor");
        item.SetGameItemType("MATERIAL");
        item.unitValue = catalyst ? ModuleCrafterEnhanceRules.CatalystUnitValue : ModuleCrafterEnhanceRules.SafeguardClipUnitValue;
        item.mayStackItemFunc = (GameItem first, GameItem second) => first != null && second != null
            && ModuleCrafterEnhanceRules.CanStackSupplies(
                first.identifier,
                second.identifier,
                GeneralHelper.IsItemOwned(first),
                GeneralHelper.IsItemOwned(second));
        item.maxSlotAddItemFunc = (GameItem _, GameInventory _, SlotMarker _) => 99;
        return item;
    }

    private static GameItem ReplaceWithRuined(GameItem original)
    {
        GameInventory parent = original.parentInventory;
        bool owned = GeneralHelper.IsItemOwned(original);
        ModuleCrafterEnhanceState.ClearForRuin(original);
        GameItem ruined = DirectoryMaster.Item(ModuleCrafterIds.Ruined);
        original.Destroy();
        if (ruined == null)
        {
            return null;
        }

        GeneralHelper.SetItemOwned(ruined, owned);
        if (!TryMoveInto(parent, ruined) && ruined.parentInventory == null)
        {
            GraphUtils.EmporiumTryAdd(ruined);
        }

        return ruined;
    }

    private static bool TryMoveInto(GameInventory inventory, GameItem item)
    {
        if (inventory == null || item == null)
        {
            return false;
        }

        // Ruined output is not a valid player input; bypass only for this replacement.
        Func<GameItem, GameInventory, bool> previous = inventory.mayInventoryAddItemFunc;
        inventory.mayInventoryAddItemFunc = null;
        int remaining = GraphUtils.TryAcceptAll(inventory, item);
        inventory.mayInventoryAddItemFunc = previous;
        return remaining == 0;
    }

    private static string SpecialPreview(ModuleCrafterChipKind kind, int nextLevel)
    {
        if (!ModuleCrafterExpansion.IsSpecialized(kind))
        {
            return "";
        }

        int currentLevel = Math.Max(0, nextLevel - 1);
        string current = ModuleCrafterLocalization.SpecialEffectSummary(kind, currentLevel);
        string next = ModuleCrafterLocalization.SpecialEffectSummary(kind, nextLevel);
        return ModuleCrafterSpecialEffectRules.StageForLevel(currentLevel) != ModuleCrafterSpecialEffectRules.StageForLevel(nextLevel)
            ? ModuleCrafterLocalization.T("effect_next", current, next)
            : ModuleCrafterLocalization.T("effect_current", next);
    }

    private static string StatPreview(GameItem item, int currentLevel, int nextLevel)
    {
        ModuleCrafterEnhanceState.GetBaseStatsForPreview(item, out int performance, out int efficiency, out int quality);
        return ModuleCrafterLocalization.T(
            "enhance_stats",
            ModuleCrafterEnhanceRules.EnhanceStat(performance, currentLevel),
            ModuleCrafterEnhanceRules.EnhanceStat(efficiency, currentLevel),
            ModuleCrafterEnhanceRules.EnhanceStat(quality, currentLevel),
            ModuleCrafterEnhanceRules.EnhanceStat(performance, nextLevel),
            ModuleCrafterEnhanceRules.EnhanceStat(efficiency, nextLevel),
            ModuleCrafterEnhanceRules.EnhanceStat(quality, nextLevel));
    }

    private static string BreakPreview(GameItem item, int level)
    {
        ModuleCrafterEnhanceState.GetBaseStatsForPreview(item, out int performance, out int efficiency, out int quality);
        bool current = ModuleCrafterEnhanceRules.BreaksVanillaCap(item.identifier, level, performance, efficiency, quality);
        bool next = ModuleCrafterEnhanceRules.BreaksVanillaCap(item.identifier, level + 1, performance, efficiency, quality);
        if (current && next)
        {
            return ModuleCrafterLocalization.T("enhance_break_both");
        }

        return next ? ModuleCrafterLocalization.T("enhance_break_next") : "";
    }

    private static string FailurePreview(int fromLevel, bool hasClip)
    {
        if (fromLevel <= 2)
        {
            return ModuleCrafterLocalization.T("enhance_failure_guaranteed");
        }

        if (fromLevel <= 7)
        {
            return ModuleCrafterLocalization.T("enhance_failure_stay");
        }

        if (fromLevel <= 9)
        {
            return ModuleCrafterLocalization.T("enhance_failure_drop1");
        }

        if (fromLevel == 10)
        {
            return ModuleCrafterLocalization.T("enhance_failure_drop3");
        }

        return ModuleCrafterLocalization.T(hasClip ? "enhance_failure_clip" : "enhance_failure_ruin");
    }

    private static string ExchangeSuffix(GameGridInventory materials)
    {
        return ModuleCrafterLocalization.T("exchange_hint", CountUnits(materials, ModuleCrafterIds.Scrap));
    }

    private static string ResultKey(ModuleCrafterEnhanceResultKind kind)
    {
        return kind switch
        {
            ModuleCrafterEnhanceResultKind.Success => "enhance_result_success",
            ModuleCrafterEnhanceResultKind.Stay => "enhance_result_stay",
            ModuleCrafterEnhanceResultKind.Drop => "enhance_result_drop",
            ModuleCrafterEnhanceResultKind.Ruined => "enhance_result_ruined",
            ModuleCrafterEnhanceResultKind.SafeguardReset => "enhance_result_clip",
            _ => "enhance_result_stay"
        };
    }

    private static IEnumerable<GameItem> SlotItems(GameInventory inventory)
    {
        if (inventory == null || inventory.childItems == null)
        {
            yield break;
        }

        foreach (GameItem item in inventory.childItems)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static int NextExclusive(int exclusive)
    {
        return RNG.GetRandomIntExculsive(0, exclusive);
    }
}
