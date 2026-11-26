#nullable disable
using System;
using HarmonyLib;

public static class ModuleCrafterEnhanceState
{
    public const string LevelTag = "MODULE_ENHANCE_LEVEL_INT";
    public const string BasePerformanceTag = "MODULE_ENHANCE_BASE_PERFORMANCE_INT";
    public const string BaseEfficiencyTag = "MODULE_ENHANCE_BASE_EFFICIENCY_INT";
    public const string BaseQualityTag = "MODULE_ENHANCE_BASE_QUALITY_INT";
    public const string BaseValueTag = "MODULE_ENHANCE_BASE_VALUE";
    public const string PriceEstimatedTag = "MODULE_ENHANCE_PRICE_ESTIMATED_BOOL";

    private const string PerformanceTag = "BONUS_PERCENTAGE_PERFORMANCE_INT";
    private const string EfficiencyTag = "BONUS_PERCENTAGE_EFFICIENCY_INT";
    private const string QualityTag = "BONUS_PERCENTAGE_QUALITY_INT";
    private const string LearningPerformanceTag = "LEARNING_ALGO_MAX_PERFORMANCE_INT";
    private const string LearningEfficiencyTag = "LEARNING_ALGO_MAX_EFFICIENCY_INT";
    private const string LearningQualityTag = "LEARNING_ALGO_MAX_QUALITY_INT";

    public static bool IsIntactSupportedModule(GameItem item)
    {
        return item != null && TryGetKind(item.identifier, out _);
    }

    public static bool TryGetKind(string itemId, out ModuleCrafterChipKind kind)
    {
        foreach (ModuleCrafterChipKind value in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            if (ModuleCrafterIds.OutputModule(value) == itemId)
            {
                kind = value;
                return true;
            }
        }

        kind = ModuleCrafterChipKind.Performance;
        return false;
    }

    public static bool HasEnhancementState(GameItem item)
    {
        return item != null && item.IsTag(LevelTag);
    }

    public static int GetLevel(GameItem item)
    {
        return HasEnhancementState(item) ? ModuleCrafterEnhanceRules.ClampLevel(item.GetTagReadonly(LevelTag).GetInt()) : 0;
    }

    public static void SetLevel(GameItem item, int level)
    {
        if (item == null)
        {
            return;
        }

        EnsureBaseSnapshot(item);
        SetInt(item, LevelTag, ModuleCrafterEnhanceRules.ClampLevel(level));
        Recalculate(item);
    }

    public static void EnsureBaseSnapshot(GameItem item)
    {
        if (!IsIntactSupportedModule(item))
        {
            return;
        }

        if (!item.IsTag(LevelTag))
        {
            SetInt(item, LevelTag, 0);
        }

        EnsureBaseDimension(item, BasePerformanceTag, PerformanceTag, LearningPerformanceTag);
        EnsureBaseDimension(item, BaseEfficiencyTag, EfficiencyTag, LearningEfficiencyTag);
        EnsureBaseDimension(item, BaseQualityTag, QualityTag, LearningQualityTag);
        EnsureBaseValueSnapshot(item);
    }

    public static void Recalculate(GameItem item)
    {
        if (!IsIntactSupportedModule(item))
        {
            return;
        }

        EnsureBaseSnapshot(item);
        int level = GetLevel(item);
        bool learning = HasLearningAlgorithm(item);
        RecalculateDimension(item, BasePerformanceTag, PerformanceTag, LearningPerformanceTag, level, learning, false);
        RecalculateDimension(item, BaseEfficiencyTag, EfficiencyTag, LearningEfficiencyTag, level, learning, false);
        RecalculateDimension(item, BaseQualityTag, QualityTag, LearningQualityTag, level, learning, false);
        if (TryGetKind(item.identifier, out ModuleCrafterChipKind specialKind) && specialKind == ModuleCrafterChipKind.ChemEquipment)
        {
            SetInt(item, QualityTag, ModuleCrafterSpecialEffectRules.Evaluate(specialKind, level).QualityBonus);
        }

        ModuleEffectHelper.ResetAllTempStat(item);
        UpdateCatalogValue(item, level);
        ModuleCrafterEnhanceVisuals.Attach(item);
        item.Validate();
    }

    public static void RestoreAll()
    {
        if (PlayerStore.Instance == null)
        {
            return;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (HasEnhancementState(item) && IsIntactSupportedModule(item))
            {
                try
                {
                    Recalculate(item);
                }
                catch (Exception ex)
                {
                    ModuleCrafterMod.LogMsg("Enhancement restore failed for " + item.identifier + ": " + ex.Message);
                }
            }
        }
    }

    public static void RestoreUnenhancedForDisable()
    {
        if (PlayerStore.Instance != null)
        {
            foreach (GameItem item in PlayerStore.Instance.FindAllItem())
            {
                if (!HasEnhancementState(item) || !IsIntactSupportedModule(item))
                {
                    continue;
                }

                try
                {
                    RestoreBase(item);
                }
                catch (Exception ex)
                {
                    ModuleCrafterMod.LogMsg("Enhancement disable restore failed for " + item.identifier + ": " + ex.Message);
                }
            }
        }

        ModuleCrafterEnhanceVisuals.Shutdown();
    }

    public static void Degrade(GameItem item, int amount)
    {
        if (!HasEnhancementState(item) || !IsIntactSupportedModule(item))
        {
            return;
        }

        EnsureBaseSnapshot(item);
        SetInt(item, BasePerformanceTag, ModuleCrafterEnhanceRules.DegradeBase(item.GetTagReadonly(BasePerformanceTag).GetInt(), amount));
        SetInt(item, BaseEfficiencyTag, ModuleCrafterEnhanceRules.DegradeBase(item.GetTagReadonly(BaseEfficiencyTag).GetInt(), amount));
        SetInt(item, BaseQualityTag, ModuleCrafterEnhanceRules.DegradeBase(item.GetTagReadonly(BaseQualityTag).GetInt(), amount));
        Recalculate(item);
    }

    public static void ClearForRuin(GameItem item)
    {
        if (item == null)
        {
            return;
        }

        string[] tags =
        {
            LevelTag, BasePerformanceTag, BaseEfficiencyTag, BaseQualityTag, BaseValueTag, PriceEstimatedTag,
            "MODULE_EFFECT1_TAG", "MODULE_EFFECT2_TAG",
            LearningPerformanceTag, LearningEfficiencyTag, LearningQualityTag
        };
        foreach (string tag in tags)
        {
            item.ModifyTag(tag, delegate(TagState state) { state.Destroy(); }, skipValidate: true);
        }

        ModuleCrafterEnhanceVisuals.Detach(item);
    }

    public static int HighestEnhancedQualityLevel(GameItem machine)
    {
        int highest = 0;
        if (machine == null)
        {
            return highest;
        }

        foreach (GameItem module in MachineHelper.GetModules(machine))
        {
            if (module != null && module.GetTagReadonly(QualityTag).GetInt() > 0)
            {
                highest = Math.Max(highest, GetLevel(module));
            }
        }

        return highest;
    }

    public static int EvaporatorQualityCap(GameItem machine)
    {
        return ModuleCrafterEnhanceRules.EvaporatorQualityCap(HighestEnhancedQualityLevel(machine));
    }

    public static string FirstEffect(GameItem item)
    {
        return item != null && item.IsTag("MODULE_EFFECT1_TAG") ? item.GetTagReadonly("MODULE_EFFECT1_TAG").GetString() : null;
    }

    public static string SecondEffect(GameItem item)
    {
        return item != null && item.IsTag("MODULE_EFFECT2_TAG") ? item.GetTagReadonly("MODULE_EFFECT2_TAG").GetString() : null;
    }

    public static bool HasLearningAlgorithm(GameItem item)
    {
        return FirstEffect(item) == "MODULE_EFFECT_LEARNING_ALGORITHM" || SecondEffect(item) == "MODULE_EFFECT_LEARNING_ALGORITHM";
    }

    public static void GetBaseStatsForPreview(GameItem item, out int performance, out int efficiency, out int quality)
    {
        // Read-only preview path: old items without B/N tags must not be migrated by a refresh.
        performance = ReadBaseDimension(item, BasePerformanceTag, PerformanceTag, LearningPerformanceTag);
        efficiency = ReadBaseDimension(item, BaseEfficiencyTag, EfficiencyTag, LearningEfficiencyTag);
        quality = ReadBaseDimension(item, BaseQualityTag, QualityTag, LearningQualityTag);
    }

    private static int ReadBaseDimension(GameItem item, string baseTag, string permanentTag, string learningMaxTag)
    {
        if (item == null)
        {
            return 0;
        }

        if (item.IsTag(baseTag))
        {
            return item.GetTagReadonly(baseTag).GetInt();
        }

        int current = item.GetTagReadonly(permanentTag).GetInt();
        if (HasLearningAlgorithm(item) && current >= 0 && item.IsTag(learningMaxTag))
        {
            return item.GetTagReadonly(learningMaxTag).GetInt() / 2;
        }

        return current;
    }

    private static void EnsureBaseDimension(GameItem item, string baseTag, string permanentTag, string learningMaxTag)
    {
        if (item.IsTag(baseTag))
        {
            return;
        }

        SetInt(item, baseTag, ReadBaseDimension(item, baseTag, permanentTag, learningMaxTag));
    }

    private static void RecalculateDimension(GameItem item, string baseTag, string permanentTag, string learningMaxTag, int level, bool learning, bool restoringBase)
    {
        int baseValue = item.GetTagReadonly(baseTag).GetInt();
        int current = item.GetTagReadonly(permanentTag).GetInt();
        int value;
        if (learning && baseValue > 0)
        {
            int cap = restoringBase ? baseValue * 2 : ModuleCrafterEnhanceRules.LearningCap(baseValue, level);
            value = Math.Clamp(current, 0, cap);
            SetInt(item, learningMaxTag, cap);
        }
        else
        {
            value = restoringBase ? baseValue : ModuleCrafterEnhanceRules.EnhanceStat(baseValue, level);
        }

        SetInt(item, permanentTag, value);
    }

    private static void RestoreBase(GameItem item)
    {
        EnsureBaseSnapshot(item);
        bool learning = HasLearningAlgorithm(item);
        RecalculateDimension(item, BasePerformanceTag, PerformanceTag, LearningPerformanceTag, 0, learning, true);
        RecalculateDimension(item, BaseEfficiencyTag, EfficiencyTag, LearningEfficiencyTag, 0, learning, true);
        RecalculateDimension(item, BaseQualityTag, QualityTag, LearningQualityTag, 0, learning, true);
        ModuleEffectHelper.ResetAllTempStat(item);
        UpdateCatalogValue(item, 0);
        ModuleCrafterEnhanceVisuals.Detach(item);
        item.Validate();
    }

    public static long GetBaseValue(GameItem item)
    {
        if (item != null && item.IsTag(BaseValueTag))
        {
            return item.GetTagReadonly(BaseValueTag).GetLong();
        }

        return ModuleCrafterEnhanceRules.CatalogValue(item.identifier, item.spritePath, 0, FirstEffect(item), SecondEffect(item));
    }

    private static void EnsureBaseValueSnapshot(GameItem item)
    {
        bool invalidSpecialSnapshot = TryGetKind(item.identifier, out ModuleCrafterChipKind kind)
            && ModuleCrafterExpansion.IsSpecialized(kind)
            && item.IsTag(BaseValueTag)
            && item.GetTagReadonly(BaseValueTag).GetLong() <= 1;
        if (item.IsTag(BaseValueTag) && !invalidSpecialSnapshot)
        {
            return;
        }

        // 3.0.0 mistakenly wrote max(1, 0) for native specialized modules.
        // Rebuild that invalid snapshot from the native catalog instead of
        // preserving it forever; valid legacy snapshots remain untouched.
        long value = invalidSpecialSnapshot
            ? ModuleCrafterEnhanceRules.CatalogValue(item.identifier, item.spritePath, 0, FirstEffect(item), SecondEffect(item))
            : GetBaseValue(item);
        item.ModifyTag(BaseValueTag, delegate(TagState state) { state.Enable().SetLong(value); });
        if (invalidSpecialSnapshot)
        {
            ModuleCrafterMod.LogMsg("Repaired invalid specialized module base value for " + item.identifier + ": " + value + ".");
        }
    }

    private static void UpdateCatalogValue(GameItem item, int level)
    {
        item.unitValue = ModuleCrafterEnhanceRules.CatalogValueFromBase(
            GetBaseValue(item),
            level,
            FirstEffect(item),
            SecondEffect(item),
            ModuleCrafterNetworkUpgrades.IsMasterUnlocked() && level >= 1);
    }

    private static void SetInt(GameItem item, string tag, int value)
    {
        item.ModifyTag(tag, delegate(TagState state) { state.Enable().SetInt(value); });
    }
}

[HarmonyPatch(typeof(ModuleEffectHelper), nameof(ModuleEffectHelper.Degrade))]
internal static class ModuleCrafterEnhanceDegradePatch
{
    private static bool Prefix(GameItem gameItem, int amount)
    {
        if (!ModuleCrafterEnhanceState.HasEnhancementState(gameItem)
            || !ModuleCrafterEnhanceState.IsIntactSupportedModule(gameItem))
        {
            return true;
        }

        ModuleCrafterEnhanceState.Degrade(gameItem, amount);
        return false;
    }
}
