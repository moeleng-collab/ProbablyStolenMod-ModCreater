#nullable disable
using System;

public enum ModuleCrafterChipKind
{
    Performance,
    Efficiency,
    Quality,
    Overclock,
    Eco,
    Fineness,
    Streamlining,
    NeuralCapped,
    NeuralUncapped
}

public enum ModuleCrafterTier
{
    Common,
    Advanced,
    NeuralCapped,
    NeuralUncapped
}

public enum ModuleCrafterFailureOutput
{
    Corrupt,
    Ruined
}

public enum ModuleCrafterRefundKind
{
    None,
    Ticket,
    Core
}


public enum ModuleCrafterJacksonStockKey
{
    Machine = 0,
    Performance = 1,
    Efficiency = 2,
    Quality = 3,
    Overclock = 4,
    Eco = 5,
    Fineness = 6,
    Streamlining = 7,
    Ticket = 8,
    Core = 9,
    Enhancer = 10,
    Catalyst = 11,
    SafeguardClip = 12
}


public readonly struct ModuleCrafterMaterialNeed
{
    public ModuleCrafterMaterialNeed(string itemId, int count, bool allowCorruptOrRuinedHousing = false)
    {
        ItemId = itemId;
        Count = count;
        AllowCorruptOrRuinedHousing = allowCorruptOrRuinedHousing;
    }

    public string ItemId { get; }
    public int Count { get; }
    public bool AllowCorruptOrRuinedHousing { get; }
}

public sealed class ModuleCrafterRecipe
{
    public ModuleCrafterRecipe(
        ModuleCrafterChipKind kind,
        ModuleCrafterTier tier,
        int baseNights,
        int baseSuccessPercent,
        int softCapPercent,
        int failureCorruptPercent,
        ModuleCrafterMaterialNeed[] required)
    {
        Kind = kind;
        Tier = tier;
        BaseNights = baseNights;
        BaseSuccessPercent = baseSuccessPercent;
        SoftCapPercent = softCapPercent;
        FailureCorruptPercent = failureCorruptPercent;
        Required = required;
    }

    public ModuleCrafterChipKind Kind { get; }
    public ModuleCrafterTier Tier { get; }
    public int BaseNights { get; }
    public int BaseSuccessPercent { get; }
    public int SoftCapPercent { get; }
    public int FailureCorruptPercent { get; }
    public ModuleCrafterMaterialNeed[] Required { get; }
}


public readonly struct ModuleCrafterMaterialPlan
{
    public static ModuleCrafterMaterialPlan Missing { get; } = new ModuleCrafterMaterialPlan(false, 0, false, System.Array.Empty<int>());

    public ModuleCrafterMaterialPlan(bool hasRequired, int extraElectronics, bool usedIntactHousing, int[] consumedIndices)
        : this(hasRequired, extraElectronics, usedIntactHousing, consumedIndices, ModuleCrafterRefundKind.None, System.Array.Empty<int>())
    {
    }

    public ModuleCrafterMaterialPlan(
        bool hasRequired,
        int extraElectronics,
        bool usedIntactHousing,
        int[] consumedIndices,
        ModuleCrafterRefundKind refundKind,
        int[] refundLockIndices)
    {
        HasRequired = hasRequired;
        ExtraElectronics = extraElectronics;
        UsedIntactHousing = usedIntactHousing;
        ConsumedIndices = consumedIndices ?? System.Array.Empty<int>();
        RefundKind = refundKind;
        RefundLockIndices = refundLockIndices ?? System.Array.Empty<int>();
    }

    public bool HasRequired { get; }
    public int ExtraElectronics { get; }
    public bool UsedIntactHousing { get; }
    public int[] ConsumedIndices { get; }
    public ModuleCrafterRefundKind RefundKind { get; }
    public int[] RefundLockIndices { get; }
}


public sealed class ModuleCrafterJacksonDayOffer
{
    public int Day;
    public int ListedMask;
    public int TakenMask;

    public bool IsListed(ModuleCrafterJacksonStockKey key)
    {
        return (ListedMask & (1 << (int)key)) != 0;
    }

    public bool IsTaken(ModuleCrafterJacksonStockKey key)
    {
        return (TakenMask & (1 << (int)key)) != 0;
    }

    public void SetListed(ModuleCrafterJacksonStockKey key, bool listed)
    {
        int bit = 1 << (int)key;
        if (listed)
        {
            ListedMask |= bit;
        }
        else
        {
            ListedMask &= ~bit;
        }
    }

    public void MarkTaken(ModuleCrafterJacksonStockKey key)
    {
        TakenMask |= 1 << (int)key;
    }

    public string Encode()
    {
        return Day + ":" + ListedMask + ":" + TakenMask;
    }

    public static bool TryParse(string text, out ModuleCrafterJacksonDayOffer offer)
    {
        offer = null;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        string[] parts = text.Split(':');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out int day) ||
            !int.TryParse(parts[1], out int listed) ||
            !int.TryParse(parts[2], out int taken))
        {
            return false;
        }

        offer = new ModuleCrafterJacksonDayOffer
        {
            Day = day,
            ListedMask = listed,
            TakenMask = taken
        };
        return true;
    }
}

public static class ModuleCrafterIds
{
    public const string Machine = "module_crafter";
    public const string MachineTag = "MODULE_CRAFTER_MACHINE_TAG";
    public const string Enhancer = "module_crafter_enhancer";
    public const string EnhancerTag = "MODULE_CRAFTER_ENHANCER_TAG";
    public const string Scrap = "scrap_metal";
    public const string ChipTag = "MODULE_CRAFTER_CHIP_TAG";
    public const string ChipPrefix = "module_crafter_chip_";
    public const string Electronic = "common_electronic";
    public const string Battery = "energy_credit";
    public const string Performance = "system_module_performance";
    public const string Efficiency = "system_module_efficiancy";
    public const string Quality = "system_module_quality";
    public const string Overclock = "system_module_overclock";
    public const string Eco = "system_module_eco";
    public const string Fineness = "system_module_fineness";
    public const string Streamlining = "system_module_streamlining";
    public const string Corrupt = "system_module_corrupt";
    public const string Ruined = "system_module_ruined";
    public const string NeuralCapped = "system_capped_neural_core";
    public const string NeuralUncapped = "system_uncapped_neural_core";
    public const string RefundTicket = "module_crafter_refund_ticket";
    public const string RefundCore = "module_crafter_refund_core";
    public const string EnhanceCatalyst = "module_crafter_enhance_catalyst";
    public const string SafeguardClip = "module_crafter_safeguard_clip";

    public static string Chip(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.Performance => ChipPrefix + "performance",
            ModuleCrafterChipKind.Efficiency => ChipPrefix + "efficiancy",
            ModuleCrafterChipKind.Quality => ChipPrefix + "quality",
            ModuleCrafterChipKind.Overclock => ChipPrefix + "overclock",
            ModuleCrafterChipKind.Eco => ChipPrefix + "eco",
            ModuleCrafterChipKind.Fineness => ChipPrefix + "fineness",
            ModuleCrafterChipKind.Streamlining => ChipPrefix + "streamlining",
            ModuleCrafterChipKind.NeuralCapped => ChipPrefix + "neural_capped",
            ModuleCrafterChipKind.NeuralUncapped => ChipPrefix + "neural_uncapped",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public static bool TryGetChipKind(string itemId, out ModuleCrafterChipKind kind)
    {
        foreach (ModuleCrafterChipKind value in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            if (Chip(value) == itemId)
            {
                kind = value;
                return true;
            }
        }

        kind = ModuleCrafterChipKind.Performance;
        return false;
    }

    public static string OutputModule(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.Performance => Performance,
            ModuleCrafterChipKind.Efficiency => Efficiency,
            ModuleCrafterChipKind.Quality => Quality,
            ModuleCrafterChipKind.Overclock => Overclock,
            ModuleCrafterChipKind.Eco => Eco,
            ModuleCrafterChipKind.Fineness => Fineness,
            ModuleCrafterChipKind.Streamlining => Streamlining,
            ModuleCrafterChipKind.NeuralCapped => NeuralCapped,
            ModuleCrafterChipKind.NeuralUncapped => NeuralUncapped,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}


public struct ModuleCrafterMerchantOffer
{
    public int MachineCount;
    public int EnhancerCount;
    public int TicketCount;
    public int ElectronicCount;
    public int CorruptHousingCount;
    public bool PerformanceChip;
    public bool EfficiencyChip;
    public bool QualityChip;
    public bool OverclockChip;
    public bool EcoChip;
    public bool FinenessChip;
    public bool StreamliningChip;
    public bool NeuralCappedChip;
    public bool NeuralUncappedChip;
    public bool RefundCore;
}

public struct ModuleCrafterMerchantPityState
{
    public int NeuralCappedMisses;
    public int NeuralUncappedMisses;
    public int RefundCoreMisses;
}

public static class ModuleCrafterRules
{
    public const int GridSize = 16;
    public const int MachineShapeWidth = 3;
    public const string FullMachineShapeData = "111111111";
    // Vanilla system_module_quality bonus is +8; with MACHINE_QUALITY_RATIO_INT=1,
    // GetAjustedQuality is 8. k=2 makes one standard quality module +16, near the +15 target.
    public const int QualityBonusK = 2;
    public const int VanillaStandardQualityBonus = 8;
    public const int MinNights = 1;
    public const int DefaultBaseProcessingSpeed = 33;

    public const int MachineStockChanceIfUnowned = 70;
    public const int MachineStockChanceIfOwned = 15;
    public const int CommonChipJacksonChance = 50;
    public const int CommonChipCrateChance = 15;
    public const int CommonChipScrapyardChance = 8;
    public const int AdvancedChipJacksonChance = 50;
    public const int AdvancedChipCrateChance = 5;
    public const int AdvancedChipJacksonMinDay = 21;
    public const int RefundTicketJacksonChance = 50;
    public const int RefundCoreJacksonChance = 20;
    public const int RefundCoreJacksonMinDay = 21;
    public const int CatalystJacksonChance = 50;
    public const int SafeguardClipJacksonChance = 20;
    public const int RetiredMerchantWeekday = 1;
    public const int RetiredMerchantExtraTicketChance = 75;
    public const int RetiredMerchantExtraElectronicChance = 75;
    public const int RetiredMerchantExtraHousingChance = 50;
    public const int RetiredMerchantAdvancedChipChance = 50;
    public const int RetiredMerchantAdvancedMinDay = 21;
    public const int RetiredMerchantBaseTickets = 1;
    public const int RetiredMerchantBaseElectronics = 2;
    public const int RetiredMerchantBaseHousing = 1;
    public const int RetiredMerchantPityStartChance = 20;
    public const int RetiredMerchantPityIncrement = 20;
    public const int RetiredMerchantPityMinDay = 21;

    public const int NeuralCappedThiefChance = 8;
    public const int NeuralCappedUndergroundChance = 8;
    public const int NeuralUncappedSmugglerChance = 5;
    public const int NeuralUncappedMinDay = 35;

    // Provisional shop prices (数值策划 has not locked these yet).
    // Common modules sell for 70, advanced for 100-125, capped neural 250,
    // uncapped 1500; the desequencer is 350. Chips are reusable, so they
    // sit above a single module of the same tier.
    public const int MachineUnitValue = 400;
    public const int CommonChipUnitValue = 200;
    public const int AdvancedChipUnitValue = 400;
    public const int NeuralCappedChipUnitValue = 900;
    public const int NeuralUncappedChipUnitValue = 2800;
    public const int RefundTicketUnitValue = 60;
    public const int RefundCoreUnitValue = 1200;

    public static ModuleCrafterRecipe GetRecipe(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.Performance => Common(kind),
            ModuleCrafterChipKind.Efficiency => Common(kind),
            ModuleCrafterChipKind.Quality => Common(kind),
            ModuleCrafterChipKind.Overclock => new ModuleCrafterRecipe(kind, ModuleCrafterTier.Advanced, 2, 55, 95, 60,
                new[] { Need(ModuleCrafterIds.Performance, 1), Need(ModuleCrafterIds.Electronic, 3) }),
            ModuleCrafterChipKind.Eco => new ModuleCrafterRecipe(kind, ModuleCrafterTier.Advanced, 2, 55, 95, 60,
                new[] { Need(ModuleCrafterIds.Efficiency, 1), Need(ModuleCrafterIds.Electronic, 3) }),
            ModuleCrafterChipKind.Fineness => new ModuleCrafterRecipe(kind, ModuleCrafterTier.Advanced, 2, 55, 95, 60,
                new[] { Need(ModuleCrafterIds.Quality, 1), Need(ModuleCrafterIds.Electronic, 3) }),
            ModuleCrafterChipKind.Streamlining => new ModuleCrafterRecipe(kind, ModuleCrafterTier.Advanced, 2, 55, 95, 60,
                new[] { Need(ModuleCrafterIds.Performance, 1), Need(ModuleCrafterIds.Efficiency, 1), Need(ModuleCrafterIds.Electronic, 2) }),
            ModuleCrafterChipKind.NeuralCapped => new ModuleCrafterRecipe(kind, ModuleCrafterTier.NeuralCapped, 3, 30, 80, 40,
                new[] { Need(ModuleCrafterIds.Fineness, 1), Need(ModuleCrafterIds.Electronic, 4) }),
            ModuleCrafterChipKind.NeuralUncapped => new ModuleCrafterRecipe(kind, ModuleCrafterTier.NeuralUncapped, 5, 25, 70, 40,
                new[] { Need(ModuleCrafterIds.NeuralCapped, 1), Need(ModuleCrafterIds.Fineness, 1), Need(ModuleCrafterIds.Electronic, 4) }),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public static int ComputeSuccessPercent(ModuleCrafterChipKind kind, int adjustedQuality, int extraElectronics, bool usedIntactHousing)
    {
        return ComputeSuccessPercent(kind, adjustedQuality, extraElectronics, usedIntactHousing, 0);
    }

    public static int ComputeSuccessPercent(ModuleCrafterChipKind kind, int adjustedQuality, int extraElectronics, bool usedIntactHousing, int highestEnhancedQualityLevel)
    {
        ModuleCrafterRecipe recipe = GetRecipe(kind);
        int raw = recipe.BaseSuccessPercent + (adjustedQuality * QualityBonusK);
        int cap = ModuleCrafterEnhanceRules.EffectiveCraftSoftCap(recipe.SoftCapPercent, highestEnhancedQualityLevel);
        return Math.Clamp(raw, 0, cap);
    }

    public static bool IsSoftCapped(ModuleCrafterChipKind kind, int adjustedQuality, int extraElectronics, bool usedIntactHousing)
    {
        return IsSoftCapped(kind, adjustedQuality, extraElectronics, usedIntactHousing, 0);
    }

    public static bool IsSoftCapped(ModuleCrafterChipKind kind, int adjustedQuality, int extraElectronics, bool usedIntactHousing, int highestEnhancedQualityLevel)
    {
        ModuleCrafterRecipe recipe = GetRecipe(kind);
        int raw = recipe.BaseSuccessPercent + (adjustedQuality * QualityBonusK);
        return raw >= ModuleCrafterEnhanceRules.EffectiveCraftSoftCap(recipe.SoftCapPercent, highestEnhancedQualityLevel);
    }

    public static int ComputeActualNights(int baseNights, int baseSpeed, int currentSpeed)
    {
        int nights = Math.Max(MinNights, baseNights);
        if (currentSpeed <= 0 || baseSpeed <= 0)
        {
            return nights;
        }

        int target = baseSpeed * nights;
        int actual = (target + currentSpeed - 1) / currentSpeed;
        return Math.Max(MinNights, actual);
    }

    public static bool CanStart(bool hasRequiredMaterials, bool isPowered)
    {
        return hasRequiredMaterials && isPowered;
    }

    public static bool ConsumeMaterialsOnCompletionOnly => true;

    public static bool RefundAllMaterialsOnCancel => true;

    public static bool RollSuccess(int successPercent, Func<int, int> nextExclusive)
    {
        return RollSuccess(successPercent, nextExclusive, out int _);
    }

    public static bool RollSuccess(int successPercent, Func<int, int> nextExclusive, out int roll)
    {
        int chance = Math.Clamp(successPercent, 0, 100);
        roll = nextExclusive(100);
        return roll < chance;
    }

    public static ModuleCrafterFailureOutput RollFailure(ModuleCrafterChipKind kind, Func<int, int> nextExclusive)
    {
        ModuleCrafterRecipe recipe = GetRecipe(kind);
        return nextExclusive(100) < recipe.FailureCorruptPercent
            ? ModuleCrafterFailureOutput.Corrupt
            : ModuleCrafterFailureOutput.Ruined;
    }

    public static bool RollStock(int percentChance, Func<int, int> nextExclusive)
    {
        int chance = Math.Clamp(percentChance, 0, 100);
        return nextExclusive(100) < chance;
    }

    public static int MachineStockChance(bool playerOwnsThisMachine)
    {
        return playerOwnsThisMachine ? MachineStockChanceIfOwned : MachineStockChanceIfUnowned;
    }

    public static bool AdvancedJacksonUnlocked(int dayNumber)
    {
        return dayNumber >= AdvancedChipJacksonMinDay;
    }

    public static bool RefundCoreJacksonUnlocked(int dayNumber)
    {
        return dayNumber >= RefundCoreJacksonMinDay;
    }

    public static ModuleCrafterJacksonDayOffer EnsureJacksonDayOffer(
        ModuleCrafterJacksonDayOffer current,
        int day,
        bool hasAnyMachine,
        bool ownsCrafter,
        Func<int, int> nextExclusive)
    {
        if (current != null && current.Day == day)
        {
            return current;
        }

        return RollJacksonDayOffer(day, hasAnyMachine, ownsCrafter, nextExclusive);
    }

    public static ModuleCrafterJacksonDayOffer RollJacksonDayOffer(
        int day,
        bool hasAnyMachine,
        bool ownsCrafter,
        Func<int, int> nextExclusive,
        bool ownsEnhancer = false)
    {
        ModuleCrafterJacksonDayOffer offer = new ModuleCrafterJacksonDayOffer { Day = day };
        if (hasAnyMachine)
        {
            offer.SetListed(
                ModuleCrafterJacksonStockKey.Machine,
                RollStock(MachineStockChance(ownsCrafter), nextExclusive));
        }

        if (!ownsCrafter)
        {
            if (offer.IsListed(ModuleCrafterJacksonStockKey.Machine))
            {
                EnsureAtLeastOneCommonJacksonChip(offer, nextExclusive);
            }

            if (hasAnyMachine)
            {
                offer.SetListed(
                    ModuleCrafterJacksonStockKey.Enhancer,
                    RollStock(MachineStockChance(ownsEnhancer), nextExclusive));
                offer.SetListed(
                    ModuleCrafterJacksonStockKey.Catalyst,
                    RollStock(CatalystJacksonChance, nextExclusive));
                offer.SetListed(
                    ModuleCrafterJacksonStockKey.SafeguardClip,
                    RollStock(SafeguardClipJacksonChance, nextExclusive));
            }

            return offer;
        }

        offer.SetListed(ModuleCrafterJacksonStockKey.Performance, RollStock(CommonChipJacksonChance, nextExclusive));
        offer.SetListed(ModuleCrafterJacksonStockKey.Efficiency, RollStock(CommonChipJacksonChance, nextExclusive));
        offer.SetListed(ModuleCrafterJacksonStockKey.Quality, RollStock(CommonChipJacksonChance, nextExclusive));
        if (AdvancedJacksonUnlocked(day))
        {
            offer.SetListed(ModuleCrafterJacksonStockKey.Overclock, RollStock(AdvancedChipJacksonChance, nextExclusive));
            offer.SetListed(ModuleCrafterJacksonStockKey.Eco, RollStock(AdvancedChipJacksonChance, nextExclusive));
            offer.SetListed(ModuleCrafterJacksonStockKey.Fineness, RollStock(AdvancedChipJacksonChance, nextExclusive));
            offer.SetListed(ModuleCrafterJacksonStockKey.Streamlining, RollStock(AdvancedChipJacksonChance, nextExclusive));
        }

        offer.SetListed(ModuleCrafterJacksonStockKey.Ticket, RollStock(RefundTicketJacksonChance, nextExclusive));
        if (RefundCoreJacksonUnlocked(day))
        {
            offer.SetListed(ModuleCrafterJacksonStockKey.Core, RollStock(RefundCoreJacksonChance, nextExclusive));
        }

        if (hasAnyMachine)
        {
            offer.SetListed(
                ModuleCrafterJacksonStockKey.Enhancer,
                RollStock(MachineStockChance(ownsEnhancer), nextExclusive));
            offer.SetListed(
                ModuleCrafterJacksonStockKey.Catalyst,
                RollStock(CatalystJacksonChance, nextExclusive));
            offer.SetListed(
                ModuleCrafterJacksonStockKey.SafeguardClip,
                RollStock(SafeguardClipJacksonChance, nextExclusive));
        }

        return offer;
    }

    public static bool HasAnyCommonJacksonChip(ModuleCrafterJacksonDayOffer offer)
    {
        return offer != null
            && (offer.IsListed(ModuleCrafterJacksonStockKey.Performance)
                || offer.IsListed(ModuleCrafterJacksonStockKey.Efficiency)
                || offer.IsListed(ModuleCrafterJacksonStockKey.Quality));
    }

    public static void EnsureAtLeastOneCommonJacksonChip(
        ModuleCrafterJacksonDayOffer offer,
        Func<int, int> nextExclusive)
    {
        if (offer == null || HasAnyCommonJacksonChip(offer))
        {
            return;
        }

        ModuleCrafterJacksonStockKey[] keys =
        {
            ModuleCrafterJacksonStockKey.Performance,
            ModuleCrafterJacksonStockKey.Efficiency,
            ModuleCrafterJacksonStockKey.Quality
        };
        int index = nextExclusive(keys.Length);
        if (index < 0 || index >= keys.Length)
        {
            index = 0;
        }

        offer.SetListed(keys[index], true);
    }

    public static bool ShouldPlaceJacksonStock(ModuleCrafterJacksonDayOffer offer, ModuleCrafterJacksonStockKey key)
    {
        return offer != null && offer.IsListed(key) && !offer.IsTaken(key);
    }

    public static string JacksonStockItemId(ModuleCrafterJacksonStockKey key)
    {
        return key switch
        {
            ModuleCrafterJacksonStockKey.Machine => ModuleCrafterIds.Machine,
            ModuleCrafterJacksonStockKey.Performance => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Performance),
            ModuleCrafterJacksonStockKey.Efficiency => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Efficiency),
            ModuleCrafterJacksonStockKey.Quality => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Quality),
            ModuleCrafterJacksonStockKey.Overclock => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Overclock),
            ModuleCrafterJacksonStockKey.Eco => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Eco),
            ModuleCrafterJacksonStockKey.Fineness => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Fineness),
            ModuleCrafterJacksonStockKey.Streamlining => ModuleCrafterIds.Chip(ModuleCrafterChipKind.Streamlining),
            ModuleCrafterJacksonStockKey.Ticket => ModuleCrafterIds.RefundTicket,
            ModuleCrafterJacksonStockKey.Core => ModuleCrafterIds.RefundCore,
            ModuleCrafterJacksonStockKey.Enhancer => ModuleCrafterIds.Enhancer,
            ModuleCrafterJacksonStockKey.Catalyst => ModuleCrafterIds.EnhanceCatalyst,
            ModuleCrafterJacksonStockKey.SafeguardClip => ModuleCrafterIds.SafeguardClip,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    public static bool TryGetJacksonStockKey(string itemId, out ModuleCrafterJacksonStockKey key)
    {
        foreach (ModuleCrafterJacksonStockKey value in (ModuleCrafterJacksonStockKey[])Enum.GetValues(typeof(ModuleCrafterJacksonStockKey)))
        {
            if (JacksonStockItemId(value) == itemId)
            {
                key = value;
                return true;
            }
        }

        key = ModuleCrafterJacksonStockKey.Machine;
        return false;
    }


    public static int ChipUnitValue(ModuleCrafterChipKind kind)
    {
        return GetRecipe(kind).Tier switch
        {
            ModuleCrafterTier.Common => CommonChipUnitValue,
            ModuleCrafterTier.Advanced => AdvancedChipUnitValue,
            ModuleCrafterTier.NeuralCapped => NeuralCappedChipUnitValue,
            ModuleCrafterTier.NeuralUncapped => NeuralUncappedChipUnitValue,
            _ => CommonChipUnitValue
        };
    }

    public static bool UncappedSmugglerUnlocked(int dayNumber)
    {
        return dayNumber >= NeuralUncappedMinDay;
    }


    public static bool IsIntactHousingId(string itemId)
    {
        return itemId == ModuleCrafterIds.Performance
            || itemId == ModuleCrafterIds.Efficiency
            || itemId == ModuleCrafterIds.Quality
            || itemId == ModuleCrafterIds.Overclock
            || itemId == ModuleCrafterIds.Eco
            || itemId == ModuleCrafterIds.Fineness
            || itemId == ModuleCrafterIds.Streamlining;
    }

    public static bool IsDamagedHousingId(string itemId)
    {
        return itemId == ModuleCrafterIds.Corrupt || itemId == ModuleCrafterIds.Ruined;
    }

    public static bool IsCraftMaterial(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        return itemId == ModuleCrafterIds.Electronic
            || itemId == ModuleCrafterIds.NeuralCapped
            || itemId == ModuleCrafterIds.NeuralUncapped
            || itemId == ModuleCrafterIds.RefundTicket
            || itemId == ModuleCrafterIds.RefundCore
            || IsIntactHousingId(itemId)
            || IsDamagedHousingId(itemId);
    }

    public static bool IsRefundItem(string itemId)
    {
        return itemId == ModuleCrafterIds.RefundTicket || itemId == ModuleCrafterIds.RefundCore;
    }

    public static ModuleCrafterRefundKind DetectRefund(System.Collections.Generic.IReadOnlyList<string> itemIds)
    {
        bool hasCore = false;
        bool hasTicket = false;
        if (itemIds == null)
        {
            return ModuleCrafterRefundKind.None;
        }

        for (int i = 0; i < itemIds.Count; i++)
        {
            if (itemIds[i] == ModuleCrafterIds.RefundCore)
            {
                hasCore = true;
            }
            else if (itemIds[i] == ModuleCrafterIds.RefundTicket)
            {
                hasTicket = true;
            }
        }

        if (hasCore)
        {
            return ModuleCrafterRefundKind.Core;
        }

        return hasTicket ? ModuleCrafterRefundKind.Ticket : ModuleCrafterRefundKind.None;
    }

    public static bool ShouldRefundMaterials(ModuleCrafterRefundKind kind, bool failed)
    {
        return failed && kind != ModuleCrafterRefundKind.None;
    }

    public static bool ShouldConsumeTicketOnStart(ModuleCrafterRefundKind kind)
    {
        return kind == ModuleCrafterRefundKind.Ticket;
    }

    public const string NightLogSuccessColor = "5c6f46";
    public const string NightLogFailureColor = "9c3738";
    public const string NightLogRefundColor = "cc7755";

    public static bool ShouldWriteCompletionNightLog(bool finishedDuringNightCycle)
    {
        return finishedDuringNightCycle;
    }

    public static string CompletionNightLogKey(bool passed, bool refundMaterials)
    {
        if (passed)
        {
            return "nightlog_success";
        }

        return refundMaterials ? "nightlog_fail_refund" : "nightlog_fail";
    }

    public static string CompletionNightLogColor(bool passed, bool refundMaterials)
    {
        if (passed)
        {
            return NightLogSuccessColor;
        }

        return refundMaterials ? NightLogRefundColor : NightLogFailureColor;
    }

    public const int CraftedModuleEffectChance = 10;
    public const int CraftedModuleEffectSlotLimit = 2;

    public static readonly string[] CraftedModuleEffectIds =
    {
        "MODULE_EFFECT_PREMIUM",
        "MODULE_EFFECT_INFERIOR",
        "MODULE_EFFECT_RECEIVER",
        "MODULE_EFFECT_TOP_MOUNT",
        "MODULE_EFFECT_SIDE_MOUNT",
        "MODULE_EFFECT_OVERVOLTED",
        "MODULE_EFFECT_DEGRADING",
        "MODULE_EFFECT_LEARNING_ALGORITHM",
        "MODULE_EFFECT_NEGATIVE_FEEDBACK"
    };

    public static bool AreCraftedModuleEffectsCompatible(string first, string second)
    {
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second) || first == second)
        {
            return false;
        }

        return !ConflictsWithCraftedModuleEffect(first, second)
            && !ConflictsWithCraftedModuleEffect(second, first);
    }

    private static bool ConflictsWithCraftedModuleEffect(string effect, string other)
    {
        switch (effect)
        {
            case "MODULE_EFFECT_PREMIUM":
                return other == "MODULE_EFFECT_INFERIOR" || other == "MODULE_EFFECT_LEARNING_ALGORITHM";
            case "MODULE_EFFECT_INFERIOR":
                return other == "MODULE_EFFECT_PREMIUM" || other == "MODULE_EFFECT_LEARNING_ALGORITHM";
            case "MODULE_EFFECT_OVERVOLTED":
            case "MODULE_EFFECT_DEGRADING":
            case "MODULE_EFFECT_NEGATIVE_FEEDBACK":
                return other == "MODULE_EFFECT_LEARNING_ALGORITHM";
            case "MODULE_EFFECT_LEARNING_ALGORITHM":
                return other == "MODULE_EFFECT_DEGRADING"
                    || other == "MODULE_EFFECT_OVERVOLTED"
                    || other == "MODULE_EFFECT_PREMIUM"
                    || other == "MODULE_EFFECT_INFERIOR"
                    || other == "MODULE_EFFECT_NEGATIVE_FEEDBACK";
            default:
                return false;
        }
    }

    public static string[] RollCraftedModuleEffects(System.Func<int, int> nextExclusive)
    {
        System.Collections.Generic.List<string> selected = new System.Collections.Generic.List<string>();
        if (nextExclusive == null)
        {
            return selected.ToArray();
        }

        for (int i = 0; i < CraftedModuleEffectIds.Length; i++)
        {
            int roll = nextExclusive(100);
            if (roll < 0 || roll >= CraftedModuleEffectChance)
            {
                continue;
            }

            if (selected.Count >= CraftedModuleEffectSlotLimit)
            {
                continue;
            }

            string candidate = CraftedModuleEffectIds[i];
            bool compatible = true;
            for (int j = 0; j < selected.Count; j++)
            {
                if (!AreCraftedModuleEffectsCompatible(selected[j], candidate))
                {
                    compatible = false;
                    break;
                }
            }

            if (compatible)
            {
                selected.Add(candidate);
            }
        }

        return selected.ToArray();
    }

    public static ModuleCrafterMaterialPlan PlanMaterials(ModuleCrafterChipKind kind, System.Collections.Generic.IReadOnlyList<string> itemIds)
    {
        ModuleCrafterRecipe recipe = GetRecipe(kind);
        bool[] used = new bool[itemIds.Count];
        System.Collections.Generic.List<int> consumed = new System.Collections.Generic.List<int>();

        foreach (ModuleCrafterMaterialNeed need in recipe.Required)
        {
            int remaining = need.Count;
            if (need.AllowCorruptOrRuinedHousing)
            {
                // Common recipes require a damaged or ruined housing. Intact modules
                // remain valid inputs only for recipes that explicitly request them.
                remaining -= TakeMatching(itemIds, used, consumed, remaining, IsDamagedHousingId);
            }
            else
            {
                remaining -= TakeMatching(itemIds, used, consumed, remaining, id => id == need.ItemId);
            }

            if (remaining > 0)
            {
                return ModuleCrafterMaterialPlan.Missing;
            }
        }

        ModuleCrafterRefundKind refundKind = DetectRefund(itemIds);
        System.Collections.Generic.List<int> refundLocks = new System.Collections.Generic.List<int>();
        for (int i = 0; i < itemIds.Count; i++)
        {
            if (IsRefundItem(itemIds[i]))
            {
                refundLocks.Add(i);
            }
        }

        return new ModuleCrafterMaterialPlan(true, 0, false, consumed.ToArray(), refundKind, refundLocks.ToArray());
    }


    public static bool ShouldVisitRetiredMerchant(int dayNumber, bool ownsCrafter)
    {
        return ownsCrafter && dayNumber != 0 && dayNumber % 7 == RetiredMerchantWeekday;
    }

    public static bool RetiredMerchantAdvancedUnlocked(int dayNumber)
    {
        return dayNumber >= RetiredMerchantAdvancedMinDay;
    }

    public static bool RetiredMerchantPityUnlocked(int dayNumber)
    {
        return dayNumber >= RetiredMerchantPityMinDay;
    }

    public static int RetiredMerchantPityChance(int missCount)
    {
        if (missCount < 0)
        {
            missCount = 0;
        }

        return Math.Clamp(RetiredMerchantPityStartChance + RetiredMerchantPityIncrement * missCount, 0, 100);
    }

    public static bool RollRetiredMerchantPity(int missCount, System.Func<int, int> nextExclusive, out int nextMissCount)
    {
        bool appeared = RollStock(RetiredMerchantPityChance(missCount), nextExclusive);
        nextMissCount = appeared ? 0 : missCount + 1;
        return appeared;
    }

    public static ModuleCrafterMerchantOffer RollRetiredMerchantOffer(int dayNumber, System.Func<int, int> nextExclusive)
    {
        return RollRetiredMerchantOffer(dayNumber, default, nextExclusive, out _);
    }

    public static ModuleCrafterMerchantOffer RollRetiredMerchantOffer(
        int dayNumber,
        ModuleCrafterMerchantPityState pity,
        System.Func<int, int> nextExclusive,
        out ModuleCrafterMerchantPityState nextPity)
    {
        ModuleCrafterMerchantOffer offer = new ModuleCrafterMerchantOffer
        {
            MachineCount = 1,
            EnhancerCount = 1,
            TicketCount = RetiredMerchantBaseTickets,
            ElectronicCount = RetiredMerchantBaseElectronics,
            CorruptHousingCount = RetiredMerchantBaseHousing,
            PerformanceChip = true,
            EfficiencyChip = true,
            QualityChip = true
        };
        nextPity = pity;
        if (RollStock(RetiredMerchantExtraTicketChance, nextExclusive))
        {
            offer.TicketCount++;
        }

        if (RollStock(RetiredMerchantExtraElectronicChance, nextExclusive))
        {
            offer.ElectronicCount++;
        }

        if (RollStock(RetiredMerchantExtraHousingChance, nextExclusive))
        {
            offer.CorruptHousingCount++;
        }

        if (RetiredMerchantAdvancedUnlocked(dayNumber))
        {
            offer.OverclockChip = RollStock(RetiredMerchantAdvancedChipChance, nextExclusive);
            offer.EcoChip = RollStock(RetiredMerchantAdvancedChipChance, nextExclusive);
            offer.FinenessChip = RollStock(RetiredMerchantAdvancedChipChance, nextExclusive);
            offer.StreamliningChip = RollStock(RetiredMerchantAdvancedChipChance, nextExclusive);
        }

        if (RetiredMerchantPityUnlocked(dayNumber))
        {
            offer.NeuralCappedChip = RollRetiredMerchantPity(pity.NeuralCappedMisses, nextExclusive, out nextPity.NeuralCappedMisses);
            offer.NeuralUncappedChip = RollRetiredMerchantPity(pity.NeuralUncappedMisses, nextExclusive, out nextPity.NeuralUncappedMisses);
            offer.RefundCore = RollRetiredMerchantPity(pity.RefundCoreMisses, nextExclusive, out nextPity.RefundCoreMisses);
        }

        return offer;
    }

    private static int TakeMatching(
        System.Collections.Generic.IReadOnlyList<string> itemIds,
        bool[] used,
        System.Collections.Generic.List<int> consumed,
        int count,
        System.Func<string, bool> match)
    {
        int taken = 0;
        for (int i = 0; i < itemIds.Count && taken < count; i++)
        {
            if (!used[i] && match(itemIds[i]))
            {
                used[i] = true;
                consumed.Add(i);
                taken++;
            }
        }

        return taken;
    }

    public const long ModuleBayExpansionValueBonus = 100L;

    public static long MachineCatalogValue(int moduleUpgradeStage)
    {
        int stage = moduleUpgradeStage < 0 ? 0 : moduleUpgradeStage;
        return MachineUnitValue + (ModuleBayExpansionValueBonus * stage);
    }

    // Official MayTarget ANDs every vote, so a false from the machine would veto tools
    // such as the vanilla module bay expansion kit and turbo boosters.
    public static bool ShouldAcceptMachineTarget(bool canRouteOntoMachine, bool? sourceAcceptsMachine, bool? sourceThisAcceptsMachine = null)
    {
        if (canRouteOntoMachine)
        {
            return true;
        }

        return sourceAcceptsMachine == true || sourceThisAcceptsMachine == true;
    }

    private static ModuleCrafterRecipe Common(ModuleCrafterChipKind kind)
    {
        return new ModuleCrafterRecipe(kind, ModuleCrafterTier.Common, 1, 80, 100, 80,
            new[]
            {
                new ModuleCrafterMaterialNeed(ModuleCrafterIds.Corrupt, 1, allowCorruptOrRuinedHousing: true),
                Need(ModuleCrafterIds.Electronic, 2)
            });
    }

    private static ModuleCrafterMaterialNeed Need(string itemId, int count)
    {
        return new ModuleCrafterMaterialNeed(itemId, count);
    }
}
