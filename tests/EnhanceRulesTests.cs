#nullable disable
using System;

internal static class EnhanceRulesTests
{
    public static void Run()
    {
        PositiveAndNegativeTables();
        CostsMatchLockedTablesAndFormula();
        SuccessFailureAndConsumption();
        CatalogValues();
        LearningAndDegradation();
        SoftCapsAndExchange();
        SourcesAndAnimation();
        TinkererFreeEnhancement();
        PhaseOneConstantsStayFrozen();
        SupplyStacksKeepOwnershipSeparated();
    }

    private static void PositiveAndNegativeTables()
    {
        int[] expected = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 24, 28 };
        for (int n = 0; n <= 12; n++)
        {
            Equal(expected[n], ModuleCrafterEnhanceRules.EnhancePositive(8, n));
            Equal(expected[n], ModuleCrafterEnhanceRules.EnhanceStat(8, n));
        }

        int[] negative = { -44, -44, -44, -44, -44, -44, -44, -44, -33, -33, -22, -22, -11 };
        for (int n = 0; n <= 12; n++)
        {
            Equal(negative[n], ModuleCrafterEnhanceRules.EnhanceNegative(-44, n));
            Equal(negative[n], ModuleCrafterEnhanceRules.EnhanceStat(-44, n));
        }

        Equal(1, ModuleCrafterEnhanceRules.EnhancePositive(1, 0));
        Equal(0, ModuleCrafterEnhanceRules.EnhanceStat(0, 12));
        Equal(-1, ModuleCrafterEnhanceRules.EnhanceNegative(-5, 12));
    }

    private static void CostsMatchLockedTablesAndFormula()
    {
        int[] common = { 10, 10, 10, 10, 10, 10, 10, 10, 14, 18, 27, 45 };
        int[] advanced = { 20, 20, 20, 20, 20, 20, 20, 20, 27, 36, 55, 91 };
        int[] capped = { 35, 35, 35, 35, 35, 35, 35, 35, 48, 64, 95, 159 };
        int[] uncapped = { 23, 23, 23, 30, 38, 45, 60, 83, 113, 150, 225, 375 };
        for (int from = 0; from < 12; from++)
        {
            Equal(common[from], ModuleCrafterEnhanceRules.GoldCost(ModuleCrafterChipKind.Performance, from));
            Equal(advanced[from], ModuleCrafterEnhanceRules.GoldCost(ModuleCrafterChipKind.Overclock, from));
            Equal(advanced[from], ModuleCrafterEnhanceRules.GoldCost(ModuleCrafterChipKind.Streamlining, from));
            Equal(capped[from], ModuleCrafterEnhanceRules.GoldCost(ModuleCrafterChipKind.NeuralCapped, from));
            Equal(uncapped[from], ModuleCrafterEnhanceRules.GoldCost(ModuleCrafterChipKind.NeuralUncapped, from));
            foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
            {
                Equal(ModuleCrafterEnhanceRules.GoldCost(kind, from), ModuleCrafterEnhanceRules.DesignFormulaGoldCost(kind, from));
            }
        }
    }

    private static void SuccessFailureAndConsumption()
    {
        int[] chances = { 100, 100, 100, 90, 80, 70, 60, 80, 70, 60, 55, 45 };
        for (int from = 0; from < 12; from++)
        {
            Equal(chances[from], ModuleCrafterEnhanceRules.SuccessPercent(from));
            Equal(from >= 7 ? 1 : 0, ModuleCrafterEnhanceRules.RequiredCatalysts(from));
            var success = ModuleCrafterEnhanceRules.Resolve(true, from, false);
            Equal(ModuleCrafterEnhanceResultKind.Success, success.Kind);
            Equal(from + 1, success.ToLevel);
        }

        for (int from = 3; from <= 7; from++)
        {
            Equal(from, ModuleCrafterEnhanceRules.ResolveFailure(from, false).ToLevel);
            Equal(ModuleCrafterEnhanceResultKind.Stay, ModuleCrafterEnhanceRules.ResolveFailure(from, false).Kind);
        }

        Equal(7, ModuleCrafterEnhanceRules.ResolveFailure(8, false).ToLevel);
        Equal(8, ModuleCrafterEnhanceRules.ResolveFailure(9, false).ToLevel);
        Equal(7, ModuleCrafterEnhanceRules.ResolveFailure(10, false).ToLevel);
        var ruin = ModuleCrafterEnhanceRules.ResolveFailure(11, false);
        Equal(ModuleCrafterEnhanceResultKind.Ruined, ruin.Kind);
        Equal(1, ruin.RefundCatalysts);
        var clipped = ModuleCrafterEnhanceRules.ResolveFailure(11, true);
        Equal(ModuleCrafterEnhanceResultKind.SafeguardReset, clipped.Kind);
        Equal(0, clipped.ToLevel);
        Equal(1, clipped.ConsumeSafeguardClips);
        Equal(0, clipped.RefundCatalysts);
        Equal(0, ModuleCrafterEnhanceRules.SuccessPercent(12));
        Equal(false, ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, false, 12));
        Equal(false, ModuleCrafterEnhanceRules.CanRequestEnhancement(true, true, false, false, 1));
        Equal(false, ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, true, false, 1));
        Equal(false, ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, true, 1));
    }

    private static void CatalogValues()
    {
        Equal(70, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 0, null, null));
        Equal(64, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance_l", 0, null, null));
        Equal(60, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance_c", 0, null, null));
        Equal(115, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Overclock, "overclock_s", 0, null, null));
        Equal(256, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 10, null, null));
        Equal(308, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 11, null, null));
        Equal(52, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 11, null, null)
            - ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 10, null, null));
        Equal(392, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 12, null, null));
        Equal(432, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 12, "MODULE_EFFECT_PREMIUM", null));
        Equal(85, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Streamlining, "streamlining", 0, "MODULE_EFFECT_INFERIOR", null));
        Equal(100, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance", 0, "MODULE_EFFECT_RECEIVER", "MODULE_EFFECT_TOP_MOUNT"));
        Equal(8400, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.NeuralUncapped, "uncapped", 12, null, null));
        Equal(616, ModuleCrafterEnhanceRules.CatalogValueFromBase(100, 12, null, null, masterUnlocked: true));
        Equal(100, ModuleCrafterEnhanceRules.CatalogValueFromBase(100, 0, null, null, masterUnlocked: true));
        Equal(1, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.Performance, "performance_c", 0, "MODULE_EFFECT_INFERIOR", null) - 19);
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            if (ModuleCrafterExpansion.IsSpecialized(kind))
            {
                Equal(ModuleCrafterExpansion.BaseCatalogPrice(kind),
                    ModuleCrafterEnhanceRules.BaseCatalogPrice(ModuleCrafterIds.OutputModule(kind)));
            }
        }
        Equal(150, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.OutputModule(ModuleCrafterChipKind.FurnaceBlast), "farm_module_l8", 0, null, null));
        Equal(840, ModuleCrafterEnhanceRules.CatalogValue(ModuleCrafterIds.OutputModule(ModuleCrafterChipKind.FurnaceBlast), "farm_module_l8", 12, null, null));
    }

    private static void LearningAndDegradation()
    {
        Equal(16, ModuleCrafterEnhanceRules.LearningCap(8, 0));
        Equal(56, ModuleCrafterEnhanceRules.LearningCap(8, 12));
        Equal(18, ModuleCrafterEnhanceRules.ClampLearningProgress(8, 20, 1));
        Equal(14, ModuleCrafterEnhanceRules.ClampLearningProgress(8, 14, 12));
        Equal(0, ModuleCrafterEnhanceRules.ClampLearningProgress(-44, 14, 12));
        Equal(7, ModuleCrafterEnhanceRules.DegradeBase(8, 1));
        Equal(0, ModuleCrafterEnhanceRules.DegradeBase(1, 10));
        Equal(-44, ModuleCrafterEnhanceRules.DegradeBase(-44, 1));
    }

    private static void SoftCapsAndExchange()
    {
        int[] overflow = { 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 8, 8, 10 };
        for (int n = 0; n <= 12; n++)
        {
            Equal(overflow[n], ModuleCrafterEnhanceRules.SoftCapOverflow(n));
            Equal(100 + overflow[n], ModuleCrafterEnhanceRules.EvaporatorQualityCap(n));
        }

        Equal(70, ModuleCrafterEnhanceRules.EffectiveCraftSoftCap(70, 0));
        Equal(78, ModuleCrafterEnhanceRules.EffectiveCraftSoftCap(70, 10));
        Equal(100, ModuleCrafterEnhanceRules.EffectiveCraftSoftCap(100, 12));
        Equal(95, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 80, 0, false, 0));
        Equal(100, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 80, 0, false, 8));
        Equal(0, ModuleCrafterEnhanceRules.ExchangeCatalysts(2));
        Equal(1, ModuleCrafterEnhanceRules.ExchangeCatalysts(3));
        Equal(2, ModuleCrafterEnhanceRules.ExchangeCatalysts(7));
    }

    private static void SourcesAndAnimation()
    {
        int calls = 0;
        var first = ModuleCrafterEnhanceRules.EnsureRetiredDayOffer(null, 21, _ => { calls++; return 0; });
        Equal(3, first.CatalystCount);
        Equal(1, first.SafeguardClipCount);
        Equal(2, calls);
        var cached = ModuleCrafterEnhanceRules.EnsureRetiredDayOffer(first, 21, _ => { calls++; return 99; });
        Equal(3, cached.CatalystCount);
        Equal(2, calls);
        Equal(25, ModuleCrafterEnhanceRules.AdditionalSellerChance("material_surplus_client", true));
        Equal(15, ModuleCrafterEnhanceRules.AdditionalSellerChance("material_surplus_client", false));
        Equal(25, ModuleCrafterEnhanceRules.AdditionalSellerChance("scavGenericMaterialSeller"));
        Equal(15, ModuleCrafterEnhanceRules.AdditionalSellerChance("pettyThief"));
        Equal(true, ModuleCrafterEnhanceRules.SellerItemsAreStolen("pettyThief"));
        Equal(false, ModuleCrafterEnhanceRules.SellerItemsAreStolen("scavenger"));
        Equal(8, ModuleCrafterEnhanceRules.ScavengeBonusChance);
        Equal(20, ModuleCrafterEnhanceRules.ScavengeHousingChance);
        Equal(50, ModuleCrafterEnhanceRules.ScavengeCorruptShareChance);
        int housingRolls = 0;
        Equal(ModuleCrafterIds.Corrupt, ModuleCrafterEnhanceRules.RollScavengedHousingId(Sequence(() => housingRolls++, 19, 49)));
        Equal(2, housingRolls);
        housingRolls = 0;
        Equal(ModuleCrafterIds.Ruined, ModuleCrafterEnhanceRules.RollScavengedHousingId(Sequence(() => housingRolls++, 19, 50)));
        Equal(2, housingRolls);
        housingRolls = 0;
        Equal((object)null, ModuleCrafterEnhanceRules.RollScavengedHousingId(Sequence(() => housingRolls++, 20, 0)));
        Equal(1, housingRolls);
        Equal(120, ModuleCrafterEnhanceRules.ResultFrameMilliseconds);
        Equal(500, ModuleCrafterEnhanceRules.MaxLevelFrameMilliseconds);
        Equal(6, ModuleCrafterEnhanceRules.ResultFrameCount(ModuleCrafterEnhanceResultKind.Success));
        Equal(5, ModuleCrafterEnhanceRules.ResultFrameCount(ModuleCrafterEnhanceResultKind.Stay));
        Equal(6, ModuleCrafterEnhanceRules.ResultFrameCount(ModuleCrafterEnhanceResultKind.Drop));
        Equal(7, ModuleCrafterEnhanceRules.ResultFrameCount(ModuleCrafterEnhanceResultKind.Ruined));
        Equal(8, ModuleCrafterEnhanceRules.ResultFrameCount(ModuleCrafterEnhanceResultKind.SafeguardReset));
        Equal("module_crafter_enhance_success", ModuleCrafterEnhanceRules.ResultSoundKey(ModuleCrafterEnhanceResultKind.Success));
        Equal("module_crafter_enhance_failure", ModuleCrafterEnhanceRules.ResultSoundKey(ModuleCrafterEnhanceResultKind.Stay));
        Equal("module_crafter_enhance_failure", ModuleCrafterEnhanceRules.ResultSoundKey(ModuleCrafterEnhanceResultKind.Drop));
        Equal("module_crafter_enhance_ruined", ModuleCrafterEnhanceRules.ResultSoundKey(ModuleCrafterEnhanceResultKind.Ruined));
        Equal("module_crafter_enhance_failure", ModuleCrafterEnhanceRules.ResultSoundKey(ModuleCrafterEnhanceResultKind.SafeguardReset));
        VanillaCapPreview();
    }

    private static Func<int, int> Sequence(Action onCall, params int[] values)
    {
        int index = 0;
        return _ =>
        {
            onCall();
            return values[Math.Min(index++, values.Length - 1)];
        };
    }

    private static void TinkererFreeEnhancement()
    {
        Equal(20, ModuleCrafterEnhanceRules.TinkererFreeEnhanceChance);
        int calls = 0;
        Equal(false, ModuleCrafterEnhanceRules.RollTinkererFreeEnhance(false, _ => { calls++; return 0; }));
        Equal(0, calls);
        Equal(true, ModuleCrafterEnhanceRules.RollTinkererFreeEnhance(true, _ => 19));
        Equal(false, ModuleCrafterEnhanceRules.RollTinkererFreeEnhance(true, _ => 20));

        var ruined = ModuleCrafterEnhanceRules.ResolveFailure(11, false);
        Equal(1, ModuleCrafterEnhanceRules.CatalystRefund(ruined, false));
        Equal(0, ModuleCrafterEnhanceRules.CatalystRefund(ruined, true));
        var clipped = ModuleCrafterEnhanceRules.ResolveFailure(11, true);
        Equal(1, ModuleCrafterEnhanceRules.SafeguardClipConsumption(clipped, false));
        Equal(0, ModuleCrafterEnhanceRules.SafeguardClipConsumption(clipped, true));
        Equal(0, ModuleCrafterEnhanceRules.CatalystRefund(ModuleCrafterEnhanceRules.Resolve(true, 8, false), false));
    }

    private static void VanillaCapPreview()
    {
        Equal(400, ModuleCrafterEnhanceRules.EnhancerUnitValue);
        Equal(false, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Quality, 0, 0, 0, 8));
        Equal(true, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Quality, 1, 0, 0, 8));
        Equal(false, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Performance, 0, 16, 0, 0));
        Equal(true, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Performance, 1, 16, 0, 0));
        Equal(false, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Overclock, 0, 36, 0, 0));
        Equal(true, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Overclock, 1, 36, 0, 0));
        Equal(true, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Streamlining, 1, 8, 25, 0));
        Equal(false, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Performance, 12, -44, -44, -44));
        Equal(false, ModuleCrafterEnhanceRules.BreaksVanillaCap(ModuleCrafterIds.Efficiency, 12, 0, -44, 0));
    }

    private static void PhaseOneConstantsStayFrozen()
    {
        Equal(2, ModuleCrafterRules.QualityBonusK);
        Equal(1, ModuleCrafterRules.MinNights);
        Equal(100, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Performance).SoftCapPercent);
        Equal(95, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Overclock).SoftCapPercent);
        Equal(80, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralCapped).SoftCapPercent);
        Equal(70, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).SoftCapPercent);
        Equal(false, ModuleCrafterRules.IsCraftMaterial("module_crafter_enhance_catalyst"));
        Equal(false, ModuleCrafterRules.IsCraftMaterial("module_crafter_safeguard_clip"));
    }

    private static void SupplyStacksKeepOwnershipSeparated()
    {
        Equal(true, ModuleCrafterEnhanceRules.CanStackSupplies(
            ModuleCrafterIds.EnhanceCatalyst, ModuleCrafterIds.EnhanceCatalyst, false, false));
        Equal(true, ModuleCrafterEnhanceRules.CanStackSupplies(
            ModuleCrafterIds.EnhanceCatalyst, ModuleCrafterIds.EnhanceCatalyst, true, true));
        Equal(false, ModuleCrafterEnhanceRules.CanStackSupplies(
            ModuleCrafterIds.EnhanceCatalyst, ModuleCrafterIds.EnhanceCatalyst, true, false));
        Equal(false, ModuleCrafterEnhanceRules.CanStackSupplies(
            ModuleCrafterIds.EnhanceCatalyst, ModuleCrafterIds.EnhanceCatalyst, false, true));
        Equal(false, ModuleCrafterEnhanceRules.CanStackSupplies(
            ModuleCrafterIds.EnhanceCatalyst, ModuleCrafterIds.SafeguardClip, false, false));
        Equal(false, ModuleCrafterEnhanceRules.CanStackSupplies(null, null, false, false));
    }

    private static void Equal(object expected, object actual)
    {
        if (Equals(expected, actual))
        {
            return;
        }

        if (!(expected is Enum) && !(actual is Enum) && expected is IConvertible && actual is IConvertible
            && Convert.ToDecimal(expected) == Convert.ToDecimal(actual))
        {
            return;
        }

        throw new Exception("Expected " + expected + ", got " + actual);
    }
}
