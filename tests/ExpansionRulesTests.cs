#nullable disable
using System;
using System.Collections.Generic;

internal static class ExpansionRulesTests
{
    public static void Run()
    {
        CatalogHasAllSpecializedRecipes();
        WorkloadMatchesTiersAndSpeed();
        DualRecipesFitCapacity();
        EnhancementCapsFollowPrecisionUpgrade();
        SpecialEffectsOnlyAdvanceAtTenAndTwelve();
    }

    private static void CatalogHasAllSpecializedRecipes()
    {
        var kinds = (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind));
        Equal(28, kinds.Length);
        int specialized = 0;
        foreach (ModuleCrafterChipKind kind in kinds)
        {
            if (!ModuleCrafterExpansion.IsSpecialized(kind))
            {
                continue;
            }

            specialized++;
            Assert(!string.IsNullOrEmpty(ModuleCrafterIds.Chip(kind)));
            Assert(!string.IsNullOrEmpty(ModuleCrafterIds.OutputModule(kind)));
            Assert(ModuleCrafterRules.GetRecipe(kind).UniversalRequired != null);
            Assert(ModuleCrafterExpansion.BaseCatalogPrice(kind) > 0);
        }

        Equal(19, specialized);
        Equal(3, ModuleCrafterExpansion.ContrabandLevel(ModuleCrafterChipKind.FarmNightmareCap));
        Equal(3, ModuleCrafterExpansion.ContrabandLevel(ModuleCrafterChipKind.NeuralCapped));
        Equal(4, ModuleCrafterExpansion.ContrabandLevel(ModuleCrafterChipKind.NeuralUncapped));
        Equal(0, ModuleCrafterExpansion.ContrabandLevel(ModuleCrafterChipKind.PrinterCnc));
    }

    private static void WorkloadMatchesTiersAndSpeed()
    {
        Equal(33, ModuleCrafterRules.GetWorkload(ModuleCrafterChipKind.Performance));
        Equal(66, ModuleCrafterRules.GetWorkload(ModuleCrafterChipKind.Overclock));
        Equal(99, ModuleCrafterRules.GetWorkload(ModuleCrafterChipKind.NeuralCapped));
        Equal(132, ModuleCrafterRules.GetWorkload(ModuleCrafterChipKind.AlarmTransmitter));
        Equal(165, ModuleCrafterRules.GetWorkload(ModuleCrafterChipKind.NeuralUncapped));
        Equal(2, ModuleCrafterRules.ComputeActualNightsForWorkload(66, 33));
        Equal(2, ModuleCrafterRules.ComputeActualNightsForWorkload(99, 50));
        Equal(2, ModuleCrafterRules.ComputeActualNightsForWorkload(132, 100));
        Equal(5, ModuleCrafterRules.ComputeActualNightsForWorkload(165, 0));
        Equal(5, ModuleCrafterRules.ComputeActualNightsForWorkload(165, -50));
        Equal(1, ModuleCrafterRules.ComputeActualNightsForWorkload(165, 1000));
    }

    private static void DualRecipesFitCapacity()
    {
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            if (!ModuleCrafterExpansion.IsSpecialized(kind))
            {
                continue;
            }

            ModuleCrafterRecipe recipe = ModuleCrafterRules.GetRecipe(kind);
            int themedEntities = EntityCount(recipe.Required, themed: true);
            int universalEntities = EntityCount(recipe.UniversalRequired, themed: false);
            Assert(themedEntities <= 6, kind + " themed entities: " + themedEntities);
            Assert(universalEntities <= 6, kind + " universal entities: " + universalEntities);
            Assert(Count(recipe.UniversalRequired, ModuleCrafterExpansion.UniversalComponent) <= 4, kind + " components");
            Assert(Count(recipe.Required, ModuleCrafterIds.Electronic) <= 2, kind + " themed electronics");
            Assert(Count(recipe.UniversalRequired, ModuleCrafterIds.Electronic) <= 2, kind + " universal electronics");
        }
    }

    private static int EntityCount(ModuleCrafterMaterialNeed[] requirements, bool themed)
    {
        int entities = 0;
        bool hasComponent = false;
        foreach (ModuleCrafterMaterialNeed need in requirements)
        {
            if (need.ItemId == ModuleCrafterExpansion.UniversalComponent)
            {
                hasComponent = true;
            }
            else
            {
                entities += need.Count;
            }
        }

        return entities + (hasComponent ? 1 : 0);
    }

    private static int Count(ModuleCrafterMaterialNeed[] requirements, string itemId)
    {
        int count = 0;
        foreach (ModuleCrafterMaterialNeed need in requirements)
        {
            if (need.ItemId == itemId)
            {
                count += need.Count;
            }
        }

        return count;
    }

    private static void EnhancementCapsFollowPrecisionUpgrade()
    {
        Equal(8, ModuleCrafterEnhanceRules.MaxLevelFor(ModuleCrafterChipKind.FarmNutrifruit, false));
        Equal(12, ModuleCrafterEnhanceRules.MaxLevelFor(ModuleCrafterChipKind.FarmNutrifruit, true));
        Equal(12, ModuleCrafterEnhanceRules.MaxLevelFor(ModuleCrafterChipKind.Performance, false));
        Assert(!ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, false, 8, ModuleCrafterChipKind.FarmNutrifruit, false));
        Assert(ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, false, 8, ModuleCrafterChipKind.FarmNutrifruit, true));
        Assert(!ModuleCrafterEnhanceRules.CanRequestEnhancement(true, false, false, false, 12, ModuleCrafterChipKind.FarmNutrifruit, true));
    }

    private static void SpecialEffectsOnlyAdvanceAtTenAndTwelve()
    {
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            if (!ModuleCrafterExpansion.IsSpecialized(kind))
            {
                continue;
            }

            for (int level = 0; level <= 9; level++)
            {
                Equal(ModuleCrafterSpecialEffectRules.Evaluate(kind, 0), ModuleCrafterSpecialEffectRules.Evaluate(kind, level));
            }

            Equal(ModuleCrafterSpecialEffectRules.Evaluate(kind, 10), ModuleCrafterSpecialEffectRules.Evaluate(kind, 11));
            Equal(0, ModuleCrafterSpecialEffectRules.StageForLevel(9));
            Equal(1, ModuleCrafterSpecialEffectRules.StageForLevel(10));
            Equal(2, ModuleCrafterSpecialEffectRules.StageForLevel(12));
        }

        Equal(2, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.FarmNutrifruit, 10).YieldBonus);
        Equal(3, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.FarmNutrifruit, 12).YieldBonus);
        Equal(6, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.FarmFillerweed, 10).YieldBonus);
        Equal(8, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.FarmFillerweed, 12).YieldBonus);
        Equal(20, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.ChemEquipment, 12).QualityBonus);
        Equal(ModuleCrafterDiskRetention.FiftyPercent, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.DnaStabilizer, 10).KeepDisk);
        Equal(ModuleCrafterDiskRetention.Always, ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.DnaStabilizer, 12).KeepDisk);
        Assert(ModuleCrafterSpecialEffectRules.Evaluate(ModuleCrafterChipKind.FurnaceBlast, 12).RemovePurityPenalty);
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception("Expected " + expected + ", got " + actual);
        }
    }

    private static void Assert(bool condition, string message = "Assertion failed")
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
}
