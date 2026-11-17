#nullable disable
using System;

public enum ModuleCrafterEnhanceResultKind
{
    Success,
    Stay,
    Drop,
    Ruined,
    SafeguardReset
}

public readonly struct ModuleCrafterEnhanceResult
{
    public ModuleCrafterEnhanceResult(ModuleCrafterEnhanceResultKind kind, int fromLevel)
    {
        Kind = kind;
        FromLevel = fromLevel;
        ToLevel = kind == ModuleCrafterEnhanceResultKind.Success ? fromLevel + 1
            : kind == ModuleCrafterEnhanceResultKind.Drop ? (fromLevel == 10 ? 7 : Math.Max(0, fromLevel - 1))
            : kind == ModuleCrafterEnhanceResultKind.Ruined || kind == ModuleCrafterEnhanceResultKind.SafeguardReset ? 0
            : fromLevel;
    }

    public ModuleCrafterEnhanceResultKind Kind { get; }
    public int FromLevel { get; }
    public int ToLevel { get; }
    public int RefundCatalysts => Kind == ModuleCrafterEnhanceResultKind.Ruined ? 1 : 0;
    public int ConsumeSafeguardClips => Kind == ModuleCrafterEnhanceResultKind.SafeguardReset ? 1 : 0;
}

public struct ModuleCrafterEnhanceDayOffer
{
    public int Day;
    public int CatalystCount;
    public int SafeguardClipCount;

    public string Encode()
    {
        return Day + ":" + CatalystCount + ":" + SafeguardClipCount;
    }

    public static bool TryParse(string value, out ModuleCrafterEnhanceDayOffer offer)
    {
        offer = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string[] parts = value.Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[0], out int day)
            || !int.TryParse(parts[1], out int catalysts) || !int.TryParse(parts[2], out int clips))
        {
            return false;
        }

        offer = new ModuleCrafterEnhanceDayOffer
        {
            Day = day,
            CatalystCount = Math.Clamp(catalysts, 0, 3),
            SafeguardClipCount = Math.Clamp(clips, 0, 1)
        };
        return true;
    }
}

public static class ModuleCrafterEnhanceRules
{
    public const int MaxLevel = 12;
    public const int EnhancerUnitValue = 400;
    public const int CatalystUnitValue = 20;
    public const int SafeguardClipUnitValue = 400;
    public const int ScrapPerCatalyst = 3;
    public const int RetiredCatalystChance = 50;
    public const int RetiredCatalystCount = 3;
    public const int RetiredClipChance = 10;
    public const int RetiredClipMinDay = 21;
    public const int MaterialSevereChance = 25;
    public const int MaterialNormalChance = 15;
    public const int GenericSellerChance = 25;
    public const int CharacterSellerChance = 15;
    public const int ScavengeBonusChance = 8;
    public const int ResultFrameMilliseconds = 120;
    public const int MaxLevelFrameMilliseconds = 500;

    public static bool CanStackSupplies(string firstId, string secondId, bool firstOwned, bool secondOwned)
    {
        return !string.IsNullOrEmpty(firstId) && firstId == secondId && firstOwned == secondOwned;
    }

    private static readonly int[] PositiveNumerators = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 24, 28 };
    // From +7 onward, add 30 percentage points to the original chance.
    private static readonly int[] SuccessPercents = { 100, 100, 100, 90, 80, 70, 60, 80, 70, 60, 55, 45 };
    private static readonly int[] CoefficientThousands = { 15, 15, 15, 20, 25, 30, 40, 55, 75, 100, 150, 250 };
    private static readonly int[] CommonCosts = { 10, 10, 10, 10, 10, 10, 10, 10, 14, 18, 27, 45 };
    private static readonly int[] AdvancedCosts = { 20, 20, 20, 20, 20, 20, 20, 20, 27, 36, 55, 91 };
    private static readonly int[] CappedCosts = { 35, 35, 35, 35, 35, 35, 35, 35, 48, 64, 95, 159 };
    private static readonly int[] UncappedCosts = { 23, 23, 23, 30, 38, 45, 60, 83, 113, 150, 225, 375 };
    private static readonly int[] LevelPricePercents = { 100, 103, 106, 109, 112, 116, 120, 124, 130, 136, 142, 150, 160 };

    public static int ClampLevel(int level)
    {
        return Math.Clamp(level, 0, MaxLevel);
    }

    public static bool CanRequestEnhancement(bool intactSupportedModule, bool installed, bool crafting, bool outputOccupied, int fromLevel)
    {
        return intactSupportedModule && !installed && !crafting && !outputOccupied && fromLevel >= 0 && fromLevel < MaxLevel;
    }

    public static int EnhancePositive(int baseValue, int level)
    {
        return baseValue * PositiveNumerators[ClampLevel(level)] / 8;
    }

    public static int EnhanceNegative(int baseValue, int level)
    {
        int n = ClampLevel(level);
        if (n <= 7)
        {
            return baseValue;
        }

        int divisor = n <= 9 ? 4 : n <= 11 ? 2 : 4;
        int numerator = n <= 9 ? 3 : 1;
        return baseValue * numerator / divisor;
    }

    public static int EnhanceStat(int baseValue, int level)
    {
        return baseValue > 0 ? EnhancePositive(baseValue, level) : EnhanceNegative(baseValue, level);
    }

    public static bool BreaksVanillaCap(string itemId, int level, int basePerformance, int baseEfficiency, int baseQuality)
    {
        return ExceedsVanillaCap(itemId, 0, EnhancePositive(basePerformance, level))
            || ExceedsVanillaCap(itemId, 1, EnhancePositive(baseEfficiency, level))
            || ExceedsVanillaCap(itemId, 2, EnhancePositive(baseQuality, level));
    }

    private static bool ExceedsVanillaCap(string itemId, int dimension, int value)
    {
        int cap = (itemId, dimension) switch
        {
            (ModuleCrafterIds.Performance, 0) => 16,
            (ModuleCrafterIds.Efficiency, 1) => 16,
            (ModuleCrafterIds.Quality, 2) => 8,
            (ModuleCrafterIds.Overclock, 0) => 36,
            (ModuleCrafterIds.Eco, 1) => 32,
            (ModuleCrafterIds.Fineness, 2) => 25,
            (ModuleCrafterIds.Streamlining, 0) => 8,
            (ModuleCrafterIds.Streamlining, 1) => 25,
            (ModuleCrafterIds.NeuralCapped, 0) => 100,
            (ModuleCrafterIds.NeuralCapped, 2) => 50,
            (ModuleCrafterIds.NeuralUncapped, 0) => 200,
            (ModuleCrafterIds.NeuralUncapped, 2) => 100,
            _ => 0
        };
        return cap > 0 && value > cap;
    }

    public static int SuccessPercent(int fromLevel)
    {
        if (fromLevel < 0 || fromLevel >= MaxLevel)
        {
            return 0;
        }

        return SuccessPercents[fromLevel];
    }

    public static int RequiredCatalysts(int fromLevel)
    {
        return fromLevel >= 7 && fromLevel < MaxLevel ? 1 : 0;
    }

    public static int GoldCost(ModuleCrafterChipKind kind, int fromLevel)
    {
        if (fromLevel < 0 || fromLevel >= MaxLevel)
        {
            return 0;
        }

        ModuleCrafterTier tier = ModuleCrafterRules.GetRecipe(kind).Tier;
        return tier switch
        {
            ModuleCrafterTier.Common => CommonCosts[fromLevel],
            ModuleCrafterTier.Advanced => AdvancedCosts[fromLevel],
            ModuleCrafterTier.NeuralCapped => CappedCosts[fromLevel],
            ModuleCrafterTier.NeuralUncapped => UncappedCosts[fromLevel],
            _ => 0
        };
    }

    public static int DesignFormulaGoldCost(ModuleCrafterChipKind kind, int fromLevel)
    {
        int coefficient = CoefficientThousands[Math.Clamp(fromLevel, 0, 11)];
        ModuleCrafterTier tier = ModuleCrafterRules.GetRecipe(kind).Tier;
        if (tier == ModuleCrafterTier.NeuralUncapped)
        {
            return (int)RoundAwayFromZero(1500 * coefficient, 1000);
        }

        int baseCost = tier == ModuleCrafterTier.Common ? 10 : tier == ModuleCrafterTier.Advanced ? 20 : 35;
        if (fromLevel <= 7)
        {
            return baseCost;
        }

        return Math.Max(1, (int)RoundAwayFromZero(baseCost * coefficient, 55));
    }

    public static ModuleCrafterEnhanceResult ResolveFailure(int fromLevel, bool hasSafeguardClip)
    {
        if (fromLevel >= 0 && fromLevel <= 2)
        {
            throw new ArgumentOutOfRangeException(nameof(fromLevel), "Levels 0-2 cannot fail.");
        }

        if (fromLevel == MaxLevel - 1)
        {
            return new ModuleCrafterEnhanceResult(
                hasSafeguardClip ? ModuleCrafterEnhanceResultKind.SafeguardReset : ModuleCrafterEnhanceResultKind.Ruined,
                fromLevel);
        }

        return new ModuleCrafterEnhanceResult(
            fromLevel >= 8 ? ModuleCrafterEnhanceResultKind.Drop : ModuleCrafterEnhanceResultKind.Stay,
            fromLevel);
    }

    public static ModuleCrafterEnhanceResult Resolve(bool success, int fromLevel, bool hasSafeguardClip)
    {
        return success ? new ModuleCrafterEnhanceResult(ModuleCrafterEnhanceResultKind.Success, fromLevel)
            : ResolveFailure(fromLevel, hasSafeguardClip);
    }

    public static int ExchangeCatalysts(int scrapCount)
    {
        return Math.Max(0, scrapCount) / ScrapPerCatalyst;
    }

    public static int BaseCatalogPrice(string itemId)
    {
        if (itemId == ModuleCrafterIds.NeuralUncapped)
        {
            return 1500;
        }

        if (itemId == ModuleCrafterIds.NeuralCapped)
        {
            return 250;
        }

        if (itemId == ModuleCrafterIds.Streamlining)
        {
            return 125;
        }

        if (itemId == ModuleCrafterIds.Overclock || itemId == ModuleCrafterIds.Eco || itemId == ModuleCrafterIds.Fineness)
        {
            return 100;
        }

        return itemId == ModuleCrafterIds.Performance || itemId == ModuleCrafterIds.Efficiency || itemId == ModuleCrafterIds.Quality ? 70 : 0;
    }

    public static int ShapePricePercent(string spritePath)
    {
        string shape = spritePath ?? "";
        if (shape.EndsWith("_l", StringComparison.Ordinal) || shape.EndsWith("_t", StringComparison.Ordinal))
        {
            return 92;
        }

        if (shape.EndsWith("_c", StringComparison.Ordinal) || shape.EndsWith("_c_red", StringComparison.Ordinal))
        {
            return 85;
        }

        if (shape.EndsWith("_s", StringComparison.Ordinal) || shape.EndsWith("_h", StringComparison.Ordinal))
        {
            return 115;
        }

        return 100;
    }

    public static int FixedEffectValue(string effect)
    {
        switch (effect)
        {
            case "MODULE_EFFECT_PREMIUM":
                return 40;
            case "MODULE_EFFECT_INFERIOR":
                return -40;
            case "MODULE_EFFECT_RECEIVER":
            case "MODULE_EFFECT_TOP_MOUNT":
            case "MODULE_EFFECT_SIDE_MOUNT":
                return 15;
            default:
                return 0;
        }
    }

    public static long CatalogValue(string itemId, string spritePath, int level, string firstEffect, string secondEffect)
    {
        int basePrice = BaseCatalogPrice(itemId);
        long shaped = RoundAwayFromZero(basePrice * ShapePricePercent(spritePath), 100);
        long leveled = RoundAwayFromZero(shaped * LevelPricePercents[ClampLevel(level)], 100);
        long value = leveled + FixedEffectValue(firstEffect) + FixedEffectValue(secondEffect);
        return Math.Max(1L, value);
    }

    public static int LearningCap(int unenhancedBase, int level)
    {
        return unenhancedBase > 0 ? EnhancePositive(unenhancedBase, level) * 2 : 0;
    }

    public static int ClampLearningProgress(int unenhancedBase, int currentProgress, int level)
    {
        return Math.Clamp(currentProgress, 0, LearningCap(unenhancedBase, level));
    }

    public static int DegradeBase(int unenhancedBase, int amount)
    {
        return unenhancedBase > 0 ? Math.Max(0, unenhancedBase - Math.Max(0, amount)) : unenhancedBase;
    }

    public static int SoftCapOverflow(int highestEnhancedQualityLevel)
    {
        int n = ClampLevel(highestEnhancedQualityLevel);
        return n <= 7 ? 0 : n <= 9 ? 5 : n <= 11 ? 8 : 10;
    }

    public static int EffectiveCraftSoftCap(int baseSoftCap, int highestEnhancedQualityLevel)
    {
        return Math.Min(100, baseSoftCap + SoftCapOverflow(highestEnhancedQualityLevel));
    }

    public static int EvaporatorQualityCap(int highestEnhancedQualityLevel)
    {
        return 100 + SoftCapOverflow(highestEnhancedQualityLevel);
    }

    public static string ResultSoundKey(ModuleCrafterEnhanceResultKind kind)
    {
        return kind switch
        {
            ModuleCrafterEnhanceResultKind.Success => "module_crafter_enhance_success",
            ModuleCrafterEnhanceResultKind.Ruined => "module_crafter_enhance_ruined",
            ModuleCrafterEnhanceResultKind.Stay => "module_crafter_enhance_failure",
            ModuleCrafterEnhanceResultKind.Drop => "module_crafter_enhance_failure",
            ModuleCrafterEnhanceResultKind.SafeguardReset => "module_crafter_enhance_failure",
            _ => ""
        };
    }

    public static int ResultFrameCount(ModuleCrafterEnhanceResultKind kind)
    {
        return kind switch
        {
            ModuleCrafterEnhanceResultKind.Success => 6,
            ModuleCrafterEnhanceResultKind.Stay => 5,
            ModuleCrafterEnhanceResultKind.Drop => 6,
            ModuleCrafterEnhanceResultKind.Ruined => 7,
            ModuleCrafterEnhanceResultKind.SafeguardReset => 8,
            _ => 0
        };
    }

    public static ModuleCrafterEnhanceDayOffer EnsureRetiredDayOffer(ModuleCrafterEnhanceDayOffer? current, int day, Func<int, int> nextExclusive)
    {
        if (current.HasValue && current.Value.Day == day)
        {
            return current.Value;
        }

        return new ModuleCrafterEnhanceDayOffer
        {
            Day = day,
            CatalystCount = ModuleCrafterRules.RollStock(RetiredCatalystChance, nextExclusive) ? RetiredCatalystCount : 0,
            SafeguardClipCount = day >= RetiredClipMinDay && ModuleCrafterRules.RollStock(RetiredClipChance, nextExclusive) ? 1 : 0
        };
    }

    public static int AdditionalSellerChance(string identifier, bool severeSurplus = false)
    {
        switch (identifier)
        {
            case "material_surplus_client":
                return severeSurplus ? MaterialSevereChance : MaterialNormalChance;
            case "scavGenericMaterialSeller":
            case "thiefGenericMaterialSeller":
                return GenericSellerChance;
            case "scavenger":
            case "thief":
            case "pettyThief":
                return CharacterSellerChance;
            default:
                return 0;
        }
    }

    public static bool SellerItemsAreStolen(string identifier)
    {
        return identifier == "thief" || identifier == "pettyThief" || identifier == "thiefGenericMaterialSeller";
    }

    private static long RoundAwayFromZero(long numerator, long denominator)
    {
        if (denominator <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(denominator));
        }

        if (numerator >= 0)
        {
            return (numerator + denominator / 2) / denominator;
        }

        return -((-numerator + denominator / 2) / denominator);
    }
}
