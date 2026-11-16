using System;
using System.Linq;

internal static class RulesTests
{
    private static int _failures;

    private static void Main()
    {
        RecipeTable_MatchesDesign();
        ChipIds_UseVanillaEfficiencyTypo();
        Success_AppliesQualityAndMaterialBonuses();
        Success_ClampsToSoftCap();
        Success_NegativeQualityReducesChance();
        Success_IgnoresExtraElectronics();
        Success_IntactHousingBonusOnlyForCommon();
        Success_StandardQualityModuleCalibratesNearFifteen();
        Materials_PlanRequiresHousingAndElectronics();
        Materials_OnlyAllowsCraftInputs();
        Nights_NeverDropBelowOne();
        Nights_UncappedTakesFiveWhenSpeedUnchanged();
        CanStart_RequiresMaterialsAndPower();
        Cancel_RefundsAllMaterials();
        Failure_UsesTierRates();
        Stock_UsesDesignedPercentsAndDayGates();
        Stock_JacksonOfferUsesIndependentPlacementRolls();
        Merchant_VisitScheduleAndOfferRolls();
        Merchant_PityProgressesAndResets();
        Refund_RulesAndPlanning();
        CompletionNightLog_UsesMorningReportColors();
        CraftedModuleEffects_IndependentRollsAndIncompatibility();
        if (_failures != 0)
        {
            throw new Exception(_failures + " assertion(s) failed");
        }

        Console.WriteLine("All rules assertions passed");
    }

    private static void RecipeTable_MatchesDesign()
    {
        AssertEqual(1, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Performance).BaseNights);
        AssertEqual(80, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Quality).BaseSuccessPercent);
        AssertEqual(100, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Efficiency).SoftCapPercent);
        AssertEqual(80, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Performance).FailureCorruptPercent);
        AssertEqual(2, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Overclock).BaseNights);
        AssertEqual(55, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Eco).BaseSuccessPercent);
        AssertEqual(95, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Fineness).SoftCapPercent);
        AssertEqual(60, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Streamlining).FailureCorruptPercent);
        AssertEqual(3, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralCapped).BaseNights);
        AssertEqual(30, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralCapped).BaseSuccessPercent);
        AssertEqual(80, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralCapped).SoftCapPercent);
        AssertEqual(5, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).BaseNights);
        AssertEqual(25, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).BaseSuccessPercent);
        AssertEqual(70, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).SoftCapPercent);
        AssertEqual(40, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).FailureCorruptPercent);
        AssertEqual(ModuleCrafterIds.Efficiency, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Eco).Required[0].ItemId);
        AssertEqual(true, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.Performance).Required[0].AllowCorruptOrRuinedHousing);
        AssertEqual(ModuleCrafterIds.NeuralCapped, ModuleCrafterRules.GetRecipe(ModuleCrafterChipKind.NeuralUncapped).Required[0].ItemId);
        AssertEqual(ModuleCrafterIds.Fineness, ModuleCrafterIds.OutputModule(ModuleCrafterChipKind.Fineness));
        AssertEqual(ModuleCrafterIds.NeuralUncapped, ModuleCrafterIds.OutputModule(ModuleCrafterChipKind.NeuralUncapped));
    }

    private static void ChipIds_UseVanillaEfficiencyTypo()
    {
        AssertEqual("module_crafter_chip_efficiancy", ModuleCrafterIds.Chip(ModuleCrafterChipKind.Efficiency));
        AssertEqual("system_module_efficiancy", ModuleCrafterIds.Efficiency);
    }

    private static void Success_AppliesQualityAndMaterialBonuses()
    {
        // 80 + 15*k(2) + intact housing 5 = 115, capped to 100; extra electronics are ignored
        AssertEqual(100, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Performance, 15, 2, true));
        // 55 + 10*k(2) = 75
        AssertEqual(75, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 10, 1, false));
        // 30 + 15*k(2) = 60
        AssertEqual(60, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.NeuralCapped, 15, 0, false));
    }

    private static void Success_ClampsToSoftCap()
    {
        AssertEqual(100, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Performance, 40, 5, true));
        AssertEqual(95, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 80, 5, false));
        AssertEqual(80, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.NeuralCapped, 80, 5, false));
        AssertEqual(70, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.NeuralUncapped, 80, 5, false));
        AssertEqual(true, ModuleCrafterRules.IsSoftCapped(ModuleCrafterChipKind.NeuralUncapped, 80, 5, false));
        AssertEqual(false, ModuleCrafterRules.IsSoftCapped(ModuleCrafterChipKind.NeuralUncapped, 0, 0, false));
    }

    private static void Success_NegativeQualityReducesChance()
    {
        AssertEqual(50, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Performance, -15, 0, false));
        AssertEqual(0, ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.NeuralUncapped, -40, 0, false));
    }

    private static void Success_IgnoresExtraElectronics()
    {
        int none = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 0, 0, false);
        int many = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Overclock, 0, 10, false);
        AssertEqual(55, none);
        AssertEqual(none, many);
    }

    private static void Success_IntactHousingBonusOnlyForCommon()
    {
        int commonPlain = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Quality, 0, 0, false);
        int commonIntact = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Quality, 0, 0, true);
        int advancedIntact = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Fineness, 0, 0, true);
        int advancedPlain = ModuleCrafterRules.ComputeSuccessPercent(ModuleCrafterChipKind.Fineness, 0, 0, false);
        AssertEqual(commonPlain + 5, commonIntact);
        AssertEqual(advancedPlain, advancedIntact);
    }


    private static void Success_StandardQualityModuleCalibratesNearFifteen()
    {
        AssertEqual(2, ModuleCrafterRules.QualityBonusK);
        AssertEqual(96, ModuleCrafterRules.ComputeSuccessPercent(
            ModuleCrafterChipKind.Performance,
            ModuleCrafterRules.VanillaStandardQualityBonus,
            0,
            false));
    }

    private static void Materials_PlanRequiresHousingAndElectronics()
    {
        ModuleCrafterMaterialPlan missing = ModuleCrafterRules.PlanMaterials(
            ModuleCrafterChipKind.Performance,
            new[] { ModuleCrafterIds.Electronic, ModuleCrafterIds.Electronic });
        AssertEqual(false, missing.HasRequired);

        ModuleCrafterMaterialPlan damaged = ModuleCrafterRules.PlanMaterials(
            ModuleCrafterChipKind.Performance,
            new[] { ModuleCrafterIds.Corrupt, ModuleCrafterIds.Electronic, ModuleCrafterIds.Electronic, ModuleCrafterIds.Electronic });
        AssertEqual(true, damaged.HasRequired);
        AssertEqual(false, damaged.UsedIntactHousing);
        AssertEqual(0, damaged.ExtraElectronics);
        AssertEqual(3, damaged.ConsumedIndices.Length);

        ModuleCrafterMaterialPlan intact = ModuleCrafterRules.PlanMaterials(
            ModuleCrafterChipKind.Performance,
            new[] { ModuleCrafterIds.Quality, ModuleCrafterIds.Electronic, ModuleCrafterIds.Electronic });
        AssertEqual(true, intact.HasRequired);
        AssertEqual(true, intact.UsedIntactHousing);
        AssertEqual(0, intact.ExtraElectronics);
    }

    private static void Nights_NeverDropBelowOne()
    {
        AssertEqual(1, ModuleCrafterRules.ComputeActualNights(1, 33, 99));
        AssertEqual(1, ModuleCrafterRules.ComputeActualNights(2, 33, 200));
        AssertEqual(1, ModuleCrafterRules.ComputeActualNights(5, 33, 1000));
        AssertEqual(5, ModuleCrafterRules.ComputeActualNights(5, 33, 0));
    }

    private static void Nights_UncappedTakesFiveWhenSpeedUnchanged()
    {
        AssertEqual(5, ModuleCrafterRules.ComputeActualNights(5, 33, 33));
        AssertEqual(3, ModuleCrafterRules.ComputeActualNights(3, 33, 33));
        AssertEqual(3, ModuleCrafterRules.ComputeActualNights(5, 33, 66));
        AssertEqual(2, ModuleCrafterRules.ComputeActualNights(4, 33, 66));
    }

    private static void CanStart_RequiresMaterialsAndPower()
    {
        AssertEqual(true, ModuleCrafterRules.CanStart(true, true));
        AssertEqual(false, ModuleCrafterRules.CanStart(false, true));
        AssertEqual(false, ModuleCrafterRules.CanStart(true, false));
    }

    private static void Cancel_RefundsAllMaterials()
    {
        AssertEqual(true, ModuleCrafterRules.ConsumeMaterialsOnCompletionOnly);
        AssertEqual(true, ModuleCrafterRules.RefundAllMaterialsOnCancel);
    }

    private static void Failure_UsesTierRates()
    {
        AssertEqual(ModuleCrafterFailureOutput.Corrupt, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.Performance, _ => 79));
        AssertEqual(ModuleCrafterFailureOutput.Ruined, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.Performance, _ => 80));
        AssertEqual(ModuleCrafterFailureOutput.Corrupt, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.Overclock, _ => 59));
        AssertEqual(ModuleCrafterFailureOutput.Ruined, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.Overclock, _ => 60));
        AssertEqual(ModuleCrafterFailureOutput.Corrupt, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.NeuralCapped, _ => 39));
        AssertEqual(ModuleCrafterFailureOutput.Ruined, ModuleCrafterRules.RollFailure(ModuleCrafterChipKind.NeuralUncapped, _ => 40));
        AssertEqual(true, ModuleCrafterRules.RollSuccess(70, _ => 69));
        AssertEqual(false, ModuleCrafterRules.RollSuccess(70, _ => 70));
    }

    private static void Stock_UsesDesignedPercentsAndDayGates()
    {
        AssertEqual(70, ModuleCrafterRules.MachineStockChance(false));
        AssertEqual(15, ModuleCrafterRules.MachineStockChance(true));
        AssertEqual(35, ModuleCrafterRules.CommonChipJacksonChance);
        AssertEqual(15, ModuleCrafterRules.CommonChipCrateChance);
        AssertEqual(8, ModuleCrafterRules.CommonChipScrapyardChance);
        AssertEqual(15, ModuleCrafterRules.AdvancedChipJacksonChance);
        AssertEqual(5, ModuleCrafterRules.AdvancedChipCrateChance);
        AssertEqual(false, ModuleCrafterRules.AdvancedJacksonUnlocked(20));
        AssertEqual(true, ModuleCrafterRules.AdvancedJacksonUnlocked(21));
        AssertEqual(8, ModuleCrafterRules.NeuralCappedThiefChance);
        AssertEqual(8, ModuleCrafterRules.NeuralCappedUndergroundChance);
        AssertEqual(5, ModuleCrafterRules.NeuralUncappedSmugglerChance);
        AssertEqual(false, ModuleCrafterRules.UncappedSmugglerUnlocked(34));
        AssertEqual(true, ModuleCrafterRules.UncappedSmugglerUnlocked(35));
        AssertEqual(true, ModuleCrafterRules.RollStock(8, _ => 7));
        AssertEqual(false, ModuleCrafterRules.RollStock(8, _ => 8));
        AssertEqual(400, ModuleCrafterRules.MachineUnitValue);
        AssertEqual(200, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.Performance));
        AssertEqual(200, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.Quality));
        AssertEqual(400, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.Overclock));
        AssertEqual(400, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.Streamlining));
        AssertEqual(900, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.NeuralCapped));
        AssertEqual(2800, ModuleCrafterRules.ChipUnitValue(ModuleCrafterChipKind.NeuralUncapped));
        AssertEqual(60, ModuleCrafterRules.RefundTicketUnitValue);
        AssertEqual(1200, ModuleCrafterRules.RefundCoreUnitValue);

        AssertEqual(20, ModuleCrafterRules.RefundTicketJacksonChance);
        AssertEqual(8, ModuleCrafterRules.RefundCoreJacksonChance);
        AssertEqual(false, ModuleCrafterRules.RefundCoreJacksonUnlocked(20));
        AssertEqual(true, ModuleCrafterRules.RefundCoreJacksonUnlocked(21));
    }


    private static void Materials_OnlyAllowsCraftInputs()
    {
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Electronic));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Corrupt));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Performance));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Fineness));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.NeuralCapped));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.NeuralUncapped));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.RefundTicket));
        AssertEqual(true, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.RefundCore));
        AssertEqual(false, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Battery));
        AssertEqual(false, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Performance)));
        AssertEqual(false, ModuleCrafterRules.IsCraftMaterial(ModuleCrafterIds.Machine));
        AssertEqual(false, ModuleCrafterRules.IsCraftMaterial("storage_crate"));
        AssertEqual(false, ModuleCrafterRules.IsCraftMaterial(null));
    }


    private static void Stock_JacksonOfferUsesIndependentPlacementRolls()
    {
        ModuleCrafterJacksonDayOffer none = ModuleCrafterRules.RollJacksonDayOffer(12, false, false, _ => 0);
        AssertEqual(false, none.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(false, none.IsListed(ModuleCrafterJacksonStockKey.Ticket));

        ModuleCrafterJacksonDayOffer machineFirst = ModuleCrafterRules.RollJacksonDayOffer(12, true, false, _ => 0);
        AssertEqual(true, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(true, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Performance));
        AssertEqual(false, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Efficiency));
        AssertEqual(false, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Quality));
        AssertEqual(false, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual(false, machineFirst.IsListed(ModuleCrafterJacksonStockKey.Core));

        ModuleCrafterJacksonDayOffer machineMiss = ModuleCrafterRules.RollJacksonDayOffer(12, true, false, _ => 99);
        AssertEqual(false, machineMiss.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(false, machineMiss.IsListed(ModuleCrafterJacksonStockKey.Performance));
        AssertEqual(false, machineMiss.IsListed(ModuleCrafterJacksonStockKey.Efficiency));
        AssertEqual(false, machineMiss.IsListed(ModuleCrafterJacksonStockKey.Quality));

        int rolls = 0;
        ModuleCrafterJacksonDayOffer machineEfficiency = ModuleCrafterRules.RollJacksonDayOffer(
            12,
            true,
            false,
            _ => rolls++ == 0 ? 0 : 1);
        AssertEqual(true, machineEfficiency.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(false, machineEfficiency.IsListed(ModuleCrafterJacksonStockKey.Performance));
        AssertEqual(true, machineEfficiency.IsListed(ModuleCrafterJacksonStockKey.Efficiency));
        AssertEqual(false, machineEfficiency.IsListed(ModuleCrafterJacksonStockKey.Quality));

        ModuleCrafterJacksonDayOffer early = ModuleCrafterRules.RollJacksonDayOffer(20, true, true, _ => 0);
        AssertEqual(true, early.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(true, early.IsListed(ModuleCrafterJacksonStockKey.Performance));
        AssertEqual(true, early.IsListed(ModuleCrafterJacksonStockKey.Efficiency));
        AssertEqual(true, early.IsListed(ModuleCrafterJacksonStockKey.Quality));
        AssertEqual(true, early.IsListed(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual(false, early.IsListed(ModuleCrafterJacksonStockKey.Overclock));
        AssertEqual(false, early.IsListed(ModuleCrafterJacksonStockKey.Core));

        ModuleCrafterJacksonDayOffer late = ModuleCrafterRules.RollJacksonDayOffer(21, true, true, _ => 0);
        AssertEqual(true, late.IsListed(ModuleCrafterJacksonStockKey.Overclock));
        AssertEqual(true, late.IsListed(ModuleCrafterJacksonStockKey.Eco));
        AssertEqual(true, late.IsListed(ModuleCrafterJacksonStockKey.Fineness));
        AssertEqual(true, late.IsListed(ModuleCrafterJacksonStockKey.Streamlining));
        AssertEqual(true, late.IsListed(ModuleCrafterJacksonStockKey.Core));

        ModuleCrafterJacksonDayOffer miss = ModuleCrafterRules.RollJacksonDayOffer(21, true, true, _ => 99);
        AssertEqual(false, miss.IsListed(ModuleCrafterJacksonStockKey.Machine));
        AssertEqual(false, miss.IsListed(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual(false, miss.IsListed(ModuleCrafterJacksonStockKey.Core));

        ModuleCrafterJacksonDayOffer first = ModuleCrafterRules.RollJacksonDayOffer(21, true, true, _ => 0);
        ModuleCrafterJacksonDayOffer second = ModuleCrafterRules.RollJacksonDayOffer(21, true, true, _ => 99);
        AssertEqual(true, first.IsListed(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual(false, second.IsListed(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual("module_crafter_refund_ticket", ModuleCrafterRules.JacksonStockItemId(ModuleCrafterJacksonStockKey.Ticket));
        AssertEqual("module_crafter_refund_core", ModuleCrafterRules.JacksonStockItemId(ModuleCrafterJacksonStockKey.Core));
        ModuleCrafterJacksonStockKey key;
        AssertEqual(true, ModuleCrafterRules.TryGetJacksonStockKey(ModuleCrafterIds.RefundTicket, out key));
        AssertEqual(ModuleCrafterJacksonStockKey.Ticket, key);
        AssertEqual(false, ModuleCrafterRules.TryGetJacksonStockKey(ModuleCrafterIds.Chip(ModuleCrafterChipKind.NeuralCapped), out key));
    }

    private static void Refund_RulesAndPlanning()
    {
        AssertEqual("module_crafter_refund_ticket", ModuleCrafterIds.RefundTicket);
        AssertEqual("module_crafter_refund_core", ModuleCrafterIds.RefundCore);
        AssertEqual(ModuleCrafterRefundKind.None, ModuleCrafterRules.DetectRefund(new[] { ModuleCrafterIds.Corrupt, ModuleCrafterIds.Electronic }));
        AssertEqual(ModuleCrafterRefundKind.Ticket, ModuleCrafterRules.DetectRefund(new[] { ModuleCrafterIds.RefundTicket }));
        AssertEqual(ModuleCrafterRefundKind.Core, ModuleCrafterRules.DetectRefund(new[] { ModuleCrafterIds.RefundCore }));
        AssertEqual(ModuleCrafterRefundKind.Core, ModuleCrafterRules.DetectRefund(new[] { ModuleCrafterIds.RefundTicket, ModuleCrafterIds.RefundCore }));
        AssertEqual(false, ModuleCrafterRules.ShouldRefundMaterials(ModuleCrafterRefundKind.None, true));
        AssertEqual(true, ModuleCrafterRules.ShouldRefundMaterials(ModuleCrafterRefundKind.Ticket, true));
        AssertEqual(false, ModuleCrafterRules.ShouldRefundMaterials(ModuleCrafterRefundKind.Ticket, false));
        AssertEqual(true, ModuleCrafterRules.ShouldConsumeTicketOnStart(ModuleCrafterRefundKind.Ticket));
        AssertEqual(false, ModuleCrafterRules.ShouldConsumeTicketOnStart(ModuleCrafterRefundKind.Core));
        AssertEqual(false, ModuleCrafterRules.ShouldConsumeTicketOnStart(ModuleCrafterRefundKind.None));

        var ids = new[]
        {
            ModuleCrafterIds.Corrupt,
            ModuleCrafterIds.Electronic,
            ModuleCrafterIds.Electronic,
            ModuleCrafterIds.RefundTicket,
            ModuleCrafterIds.Electronic
        };
        ModuleCrafterMaterialPlan plan = ModuleCrafterRules.PlanMaterials(ModuleCrafterChipKind.Performance, ids);
        AssertEqual(true, plan.HasRequired);
        AssertEqual(3, plan.ConsumedIndices.Length);
        AssertEqual(ModuleCrafterRefundKind.Ticket, plan.RefundKind);
        AssertEqual(1, plan.RefundLockIndices.Length);
        AssertEqual(3, plan.RefundLockIndices[0]);

        var both = new[]
        {
            ModuleCrafterIds.Corrupt,
            ModuleCrafterIds.Electronic,
            ModuleCrafterIds.Electronic,
            ModuleCrafterIds.RefundTicket,
            ModuleCrafterIds.RefundCore
        };
        ModuleCrafterMaterialPlan corePlan = ModuleCrafterRules.PlanMaterials(ModuleCrafterChipKind.Quality, both);
        AssertEqual(ModuleCrafterRefundKind.Core, corePlan.RefundKind);
        AssertEqual(2, corePlan.RefundLockIndices.Length);
        AssertEqual(false, ModuleCrafterRules.ShouldConsumeTicketOnStart(corePlan.RefundKind));
    }

    private static void CompletionNightLog_UsesMorningReportColors()
    {
        AssertEqual(true, ModuleCrafterRules.ShouldWriteCompletionNightLog(true));
        AssertEqual(false, ModuleCrafterRules.ShouldWriteCompletionNightLog(false));
        AssertEqual("nightlog_success", ModuleCrafterRules.CompletionNightLogKey(true, false));
        AssertEqual("nightlog_fail", ModuleCrafterRules.CompletionNightLogKey(false, false));
        AssertEqual("nightlog_fail_refund", ModuleCrafterRules.CompletionNightLogKey(false, true));
        AssertEqual("5c6f46", ModuleCrafterRules.CompletionNightLogColor(true, false));
        AssertEqual("9c3738", ModuleCrafterRules.CompletionNightLogColor(false, false));
        AssertEqual("cc7755", ModuleCrafterRules.CompletionNightLogColor(false, true));
    }

    private static void CraftedModuleEffects_IndependentRollsAndIncompatibility()
    {
        AssertEqual(10, ModuleCrafterRules.CraftedModuleEffectChance);
        AssertEqual(2, ModuleCrafterRules.CraftedModuleEffectSlotLimit);
        AssertEqual(9, ModuleCrafterRules.CraftedModuleEffectIds.Length);
        AssertEqual(false, ModuleCrafterRules.AreCraftedModuleEffectsCompatible("MODULE_EFFECT_PREMIUM", "MODULE_EFFECT_INFERIOR"));
        AssertEqual(false, ModuleCrafterRules.AreCraftedModuleEffectsCompatible("MODULE_EFFECT_LEARNING_ALGORITHM", "MODULE_EFFECT_PREMIUM"));
        AssertEqual(false, ModuleCrafterRules.AreCraftedModuleEffectsCompatible("MODULE_EFFECT_LEARNING_ALGORITHM", "MODULE_EFFECT_OVERVOLTED"));
        AssertEqual(true, ModuleCrafterRules.AreCraftedModuleEffectsCompatible("MODULE_EFFECT_PREMIUM", "MODULE_EFFECT_TOP_MOUNT"));
        AssertEqual(true, ModuleCrafterRules.AreCraftedModuleEffectsCompatible("MODULE_EFFECT_TOP_MOUNT", "MODULE_EFFECT_SIDE_MOUNT"));

        int[] allHit = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        string[] allHitEffects = ModuleCrafterRules.RollCraftedModuleEffects(ScriptedRolls(allHit));
        AssertEqual(2, allHitEffects.Length);
        AssertEqual("MODULE_EFFECT_PREMIUM", allHitEffects[0]);
        AssertEqual("MODULE_EFFECT_RECEIVER", allHitEffects[1]);

        int[] noneHit = { 10, 10, 10, 10, 10, 10, 10, 10, 10 };
        AssertEqual(0, ModuleCrafterRules.RollCraftedModuleEffects(ScriptedRolls(noneHit)).Length);

        int[] threshold = { 9, 10, 10, 10, 10, 10, 10, 10, 10 };
        string[] barely = ModuleCrafterRules.RollCraftedModuleEffects(ScriptedRolls(threshold));
        AssertEqual(1, barely.Length);
        AssertEqual("MODULE_EFFECT_PREMIUM", barely[0]);

        int[] topAndSide = { 99, 99, 99, 0, 0, 99, 99, 99, 99 };
        string[] mounts = ModuleCrafterRules.RollCraftedModuleEffects(ScriptedRolls(topAndSide));
        AssertEqual(2, mounts.Length);
        AssertEqual("MODULE_EFFECT_TOP_MOUNT", mounts[0]);
        AssertEqual("MODULE_EFFECT_SIDE_MOUNT", mounts[1]);

        int[] learningAndPremium = { 0, 99, 99, 99, 99, 99, 99, 0, 99 };
        string[] learningConflict = ModuleCrafterRules.RollCraftedModuleEffects(ScriptedRolls(learningAndPremium));
        AssertEqual(1, learningConflict.Length);
        AssertEqual("MODULE_EFFECT_PREMIUM", learningConflict[0]);
    }

    private static Func<int, int> ScriptedRolls(int[] rolls)
    {
        int index = 0;
        return _ =>
        {
            int value = rolls[Math.Min(index, rolls.Length - 1)];
            index++;
            return value;
        };
    }


    private static void Merchant_VisitScheduleAndOfferRolls()
    {
        AssertEqual(1, ModuleCrafterRules.RetiredMerchantWeekday);
        AssertEqual(75, ModuleCrafterRules.RetiredMerchantExtraTicketChance);
        AssertEqual(75, ModuleCrafterRules.RetiredMerchantExtraElectronicChance);
        AssertEqual(50, ModuleCrafterRules.RetiredMerchantExtraHousingChance);
        AssertEqual(50, ModuleCrafterRules.RetiredMerchantAdvancedChipChance);
        AssertEqual(false, ModuleCrafterRules.ShouldVisitRetiredMerchant(0, true));
        AssertEqual(false, ModuleCrafterRules.ShouldVisitRetiredMerchant(8, false));
        AssertEqual(true, ModuleCrafterRules.ShouldVisitRetiredMerchant(8, true));
        AssertEqual(false, ModuleCrafterRules.ShouldVisitRetiredMerchant(7, true));
        AssertEqual(false, ModuleCrafterRules.ShouldVisitRetiredMerchant(9, true));
        AssertEqual(true, ModuleCrafterRules.ShouldVisitRetiredMerchant(15, true));
        AssertEqual(false, ModuleCrafterRules.RetiredMerchantAdvancedUnlocked(20));
        AssertEqual(true, ModuleCrafterRules.RetiredMerchantAdvancedUnlocked(21));

        ModuleCrafterMerchantOffer earlyHit = ModuleCrafterRules.RollRetiredMerchantOffer(8, _ => 0);
        AssertEqual(1, earlyHit.MachineCount);
        AssertEqual(true, earlyHit.PerformanceChip);
        AssertEqual(true, earlyHit.EfficiencyChip);
        AssertEqual(true, earlyHit.QualityChip);
        AssertEqual(false, earlyHit.OverclockChip);
        AssertEqual(false, earlyHit.EcoChip);
        AssertEqual(2, earlyHit.TicketCount);
        AssertEqual(3, earlyHit.ElectronicCount);
        AssertEqual(2, earlyHit.CorruptHousingCount);
        AssertEqual(false, earlyHit.NeuralCappedChip);
        AssertEqual(false, earlyHit.NeuralUncappedChip);
        AssertEqual(false, earlyHit.RefundCore);

        ModuleCrafterMerchantOffer earlyMiss = ModuleCrafterRules.RollRetiredMerchantOffer(8, _ => 99);
        AssertEqual(1, earlyMiss.TicketCount);
        AssertEqual(2, earlyMiss.ElectronicCount);
        AssertEqual(1, earlyMiss.CorruptHousingCount);
        AssertEqual(false, earlyMiss.OverclockChip);

        ModuleCrafterMerchantOffer lateHit = ModuleCrafterRules.RollRetiredMerchantOffer(21, _ => 0);
        AssertEqual(true, lateHit.OverclockChip);
        AssertEqual(true, lateHit.EcoChip);
        AssertEqual(true, lateHit.FinenessChip);
        AssertEqual(true, lateHit.StreamliningChip);
        AssertEqual(true, lateHit.NeuralCappedChip);
        AssertEqual(true, lateHit.NeuralUncappedChip);
        AssertEqual(true, lateHit.RefundCore);

        ModuleCrafterMerchantOffer lateMiss = ModuleCrafterRules.RollRetiredMerchantOffer(21, _ => 99);
        AssertEqual(false, lateMiss.OverclockChip);
        AssertEqual(false, lateMiss.StreamliningChip);
        AssertEqual(1, lateMiss.TicketCount);
        AssertEqual(false, lateMiss.NeuralCappedChip);
        AssertEqual(false, lateMiss.NeuralUncappedChip);
        AssertEqual(false, lateMiss.RefundCore);

        ModuleCrafterMerchantOffer lateHitFull = ModuleCrafterRules.RollRetiredMerchantOffer(21, _ => 0);
        AssertEqual(true, lateHitFull.NeuralCappedChip);
        AssertEqual(true, lateHitFull.NeuralUncappedChip);
        AssertEqual(true, lateHitFull.RefundCore);
    }

    private static void Merchant_PityProgressesAndResets()
    {
        AssertEqual(20, ModuleCrafterRules.RetiredMerchantPityStartChance);
        AssertEqual(20, ModuleCrafterRules.RetiredMerchantPityIncrement);
        AssertEqual(21, ModuleCrafterRules.RetiredMerchantPityMinDay);
        AssertEqual(false, ModuleCrafterRules.RetiredMerchantPityUnlocked(20));
        AssertEqual(true, ModuleCrafterRules.RetiredMerchantPityUnlocked(21));
        AssertEqual(20, ModuleCrafterRules.RetiredMerchantPityChance(0));
        AssertEqual(40, ModuleCrafterRules.RetiredMerchantPityChance(1));
        AssertEqual(60, ModuleCrafterRules.RetiredMerchantPityChance(2));
        AssertEqual(80, ModuleCrafterRules.RetiredMerchantPityChance(3));
        AssertEqual(100, ModuleCrafterRules.RetiredMerchantPityChance(4));
        AssertEqual(100, ModuleCrafterRules.RetiredMerchantPityChance(8));

        int next;
        AssertEqual(true, ModuleCrafterRules.RollRetiredMerchantPity(0, _ => 0, out next));
        AssertEqual(0, next);
        AssertEqual(false, ModuleCrafterRules.RollRetiredMerchantPity(0, _ => 20, out next));
        AssertEqual(1, next);
        AssertEqual(true, ModuleCrafterRules.RollRetiredMerchantPity(1, _ => 0, out next));
        AssertEqual(0, next);
        AssertEqual(false, ModuleCrafterRules.RollRetiredMerchantPity(3, _ => 80, out next));
        AssertEqual(4, next);
        AssertEqual(true, ModuleCrafterRules.RollRetiredMerchantPity(4, _ => 99, out next));
        AssertEqual(0, next);

        ModuleCrafterMerchantPityState pity = new ModuleCrafterMerchantPityState
        {
            NeuralCappedMisses = 4,
            NeuralUncappedMisses = 0,
            RefundCoreMisses = 1
        };
        ModuleCrafterMerchantOffer early = ModuleCrafterRules.RollRetiredMerchantOffer(20, pity, _ => 0, out ModuleCrafterMerchantPityState unchanged);
        AssertEqual(false, early.NeuralCappedChip);
        AssertEqual(false, early.NeuralUncappedChip);
        AssertEqual(false, early.RefundCore);
        AssertEqual(4, unchanged.NeuralCappedMisses);
        AssertEqual(0, unchanged.NeuralUncappedMisses);
        AssertEqual(1, unchanged.RefundCoreMisses);

        ModuleCrafterMerchantOffer mixed = ModuleCrafterRules.RollRetiredMerchantOffer(21, pity, _ => 30, out ModuleCrafterMerchantPityState after);
        AssertEqual(true, mixed.NeuralCappedChip);
        AssertEqual(false, mixed.NeuralUncappedChip);
        AssertEqual(true, mixed.RefundCore);
        AssertEqual(0, after.NeuralCappedMisses);
        AssertEqual(1, after.NeuralUncappedMisses);
        AssertEqual(0, after.RefundCoreMisses);
    }

    private static void AssertEqual<T>(T expected, T actual)
    {
        if (!Equals(expected, actual))
        {
            _failures++;
            Console.WriteLine("FAIL expected " + expected + " but got " + actual);
        }
    }
}
