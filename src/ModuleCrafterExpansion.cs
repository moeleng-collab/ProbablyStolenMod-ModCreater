#nullable disable
using System;
using System.Collections.Generic;

public enum ModuleCrafterRecipeVariant
{
    Legacy = 0,
    Themed = 1,
    Universal = 2
}

/// <summary>
/// 3.0 recipe/content catalog. Existing 2.x recipes stay in ModuleCrafterRules;
/// this catalog only appends new content and keeps their numeric identifiers stable.
/// </summary>
public static class ModuleCrafterExpansion
{
    public const string UniversalComponent = "module_crafter_universal_component";
    public const int UniversalComponentUnitValue = 100;
    public const int UniversalComponentMaxStack = 20;

    private static readonly HashSet<string> ThemedMaterialIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "nutrifruit", "dream_cap", "kotton_fabric", "fillerweed", "hydroreed_filter",
        "common_chemical", "restricted_chemical", "flux_agent", "junk", "metal_ingot",
        "printer_plastic", "plant_dna_disk", "scrap_metal", "energy_credit",
        "farm_module_dream_cap", "printer_module_metal",
        "dna_manipulator_module_positive_filter", "dna_manipulator_module_negative_filter"
    };

    private static readonly Dictionary<ModuleCrafterChipKind, int> ContrabandLevels = new Dictionary<ModuleCrafterChipKind, int>
    {
        { ModuleCrafterChipKind.FarmNightmareCap, 3 },
        { ModuleCrafterChipKind.NeuralCapped, 3 },
        { ModuleCrafterChipKind.NeuralUncapped, 4 }
    };

    public static bool IsSpecialized(ModuleCrafterChipKind kind)
    {
        return kind >= ModuleCrafterChipKind.FarmNutrifruit;
    }

    public static int ContrabandLevel(ModuleCrafterChipKind kind)
    {
        return ContrabandLevels.TryGetValue(kind, out int level) ? level : 0;
    }

    public static bool IsContrabandChip(ModuleCrafterChipKind kind)
    {
        return ContrabandLevel(kind) > 0;
    }

    public static string Chip(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.FarmNutrifruit => "module_crafter_chip_farm_nutrifruit",
            ModuleCrafterChipKind.FarmDreamCap => "module_crafter_chip_farm_dream_cap",
            ModuleCrafterChipKind.FarmKottonLoom => "module_crafter_chip_farm_kotton_loom",
            ModuleCrafterChipKind.FarmFillerweed => "module_crafter_chip_farm_fillerweed",
            ModuleCrafterChipKind.FarmHydroreed => "module_crafter_chip_farm_hydroreed",
            ModuleCrafterChipKind.EvaporatorSolventRecovery => "module_crafter_chip_evaporator_solvent_recovery",
            ModuleCrafterChipKind.ChemEquipment => "module_crafter_chip_chem_equipment",
            ModuleCrafterChipKind.FurnaceJunk => "module_crafter_chip_furnace_junk",
            ModuleCrafterChipKind.PrinterMetal => "module_crafter_chip_printer_metal",
            ModuleCrafterChipKind.PrinterMeleeBatch => "module_crafter_chip_printer_melee_batch",
            ModuleCrafterChipKind.DnaStabilizer => "module_crafter_chip_dna_stabilizer",
            ModuleCrafterChipKind.DnaPositiveFilter => "module_crafter_chip_dna_positive_filter",
            ModuleCrafterChipKind.DnaNegativeFilter => "module_crafter_chip_dna_negative_filter",
            ModuleCrafterChipKind.FurnaceBlast => "module_crafter_chip_furnace_blast",
            ModuleCrafterChipKind.FarmNightmareCap => "module_crafter_chip_farm_nightmare_cap",
            ModuleCrafterChipKind.FarmSmuggler => "module_crafter_chip_farm_smuggler",
            ModuleCrafterChipKind.PrinterCnc => "module_crafter_chip_printer_cnc",
            ModuleCrafterChipKind.DnaPrecision => "module_crafter_chip_dna_precision",
            ModuleCrafterChipKind.AlarmTransmitter => "module_crafter_chip_alarm_transmitter",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public static string OutputModule(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.FarmNutrifruit => "farm_module_nutrifruit",
            ModuleCrafterChipKind.FarmDreamCap => "farm_module_dream_cap",
            ModuleCrafterChipKind.FarmKottonLoom => "farm_module_kotton_loom",
            ModuleCrafterChipKind.FarmFillerweed => "farm_module_fillerweed_production",
            ModuleCrafterChipKind.FarmHydroreed => "farm_module_hydroreed_filter",
            ModuleCrafterChipKind.EvaporatorSolventRecovery => "evaporator_module_solvent_recovery",
            ModuleCrafterChipKind.ChemEquipment => "chem_module",
            ModuleCrafterChipKind.FurnaceJunk => "furnace_module_junk",
            ModuleCrafterChipKind.PrinterMetal => "printer_module_metal",
            ModuleCrafterChipKind.PrinterMeleeBatch => "printer_module_melee_batch",
            ModuleCrafterChipKind.DnaStabilizer => "dna_manipulator_module_stabilizer",
            ModuleCrafterChipKind.DnaPositiveFilter => "dna_manipulator_module_positive_filter",
            ModuleCrafterChipKind.DnaNegativeFilter => "dna_manipulator_module_negative_filter",
            ModuleCrafterChipKind.FurnaceBlast => "furnace_module_blast",
            ModuleCrafterChipKind.FarmNightmareCap => "farm_module_nightmare_cap",
            ModuleCrafterChipKind.FarmSmuggler => "farm_module_smuggler",
            ModuleCrafterChipKind.PrinterCnc => "printer_module_cnc",
            ModuleCrafterChipKind.DnaPrecision => "dna_manipulator_module_precision",
            ModuleCrafterChipKind.AlarmTransmitter => "alarm_module_transmitter",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public static int ChipUnitValue(ModuleCrafterChipKind kind)
    {
        return GetRecipe(kind).Tier switch
        {
            ModuleCrafterTier.Specialized => 300,
            ModuleCrafterTier.Precision => 600,
            ModuleCrafterTier.Prototype => 900,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public static bool IsCraftMaterial(string itemId)
    {
        return itemId == UniversalComponent || ThemedMaterialIds.Contains(itemId);
    }

    public static ModuleCrafterRecipe GetRecipe(ModuleCrafterChipKind kind)
    {
        ModuleCrafterRecipe recipe = CreateRecipe(kind);
        if (recipe == null)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        return recipe;
    }

    private static ModuleCrafterRecipe CreateRecipe(ModuleCrafterChipKind kind)
    {
        const string Q = ModuleCrafterIds.Quality;
        const string E = ModuleCrafterIds.Efficiency;
        const string P = ModuleCrafterIds.Performance;
        const string O = ModuleCrafterIds.Overclock;
        const string Eco = ModuleCrafterIds.Eco;
        const string F = ModuleCrafterIds.Fineness;
        const string Elec = ModuleCrafterIds.Electronic;
        const string U = UniversalComponent;

        switch (kind)
        {
            case ModuleCrafterChipKind.FarmNutrifruit:
                return Specialized(kind, R(Q, 1, "nutrifruit", 2, Elec, 2), R(Q, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FarmDreamCap:
                return Specialized(kind, R(Eco, 1, "dream_cap", 2, "common_chemical", 1, Elec, 2), R(Eco, 1, U, 3, Elec, 2));
            case ModuleCrafterChipKind.FarmKottonLoom:
                return Specialized(kind, R(E, 1, "kotton_fabric", 2, Elec, 2), R(E, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FarmFillerweed:
                return Specialized(kind, R(Q, 1, "fillerweed", 2, Elec, 2), R(Q, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FarmHydroreed:
                return Specialized(kind, R(E, 1, "hydroreed_filter", 1, Elec, 2), R(E, 1, U, 1, Elec, 2));
            case ModuleCrafterChipKind.EvaporatorSolventRecovery:
                return Specialized(kind, R(Eco, 1, "common_chemical", 1, "flux_agent", 1, Elec, 2), R(Eco, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.ChemEquipment:
                return Specialized(kind, R(Q, 1, "common_chemical", 1, "restricted_chemical", 1, Elec, 2), R(Q, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FurnaceJunk:
                return Specialized(kind, R(Eco, 1, "junk", 4, Elec, 1), R(Eco, 1, U, 4, Elec, 2));
            case ModuleCrafterChipKind.PrinterMetal:
                return Precision(kind, R(P, 1, "metal_ingot", 1, "printer_plastic", 1, Elec, 2), R(P, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.PrinterMeleeBatch:
                return Precision(kind, R(O, 1, "scrap_metal", 2, "printer_plastic", 1, Elec, 2), R(O, 1, U, 3, Elec, 2));
            case ModuleCrafterChipKind.DnaStabilizer:
                return Precision(kind, R(Q, 1, "plant_dna_disk", 1, "common_chemical", 1, Elec, 2), R(Q, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.DnaPositiveFilter:
                return Precision(kind, R(F, 1, "plant_dna_disk", 1, Elec, 2), R(F, 1, U, 1, Elec, 2));
            case ModuleCrafterChipKind.DnaNegativeFilter:
                return Precision(kind, R(F, 1, "plant_dna_disk", 1, Elec, 2), R(F, 1, U, 1, Elec, 2));
            case ModuleCrafterChipKind.FurnaceBlast:
                return Precision(kind, R(O, 1, "metal_ingot", 1, "flux_agent", 1, Elec, 2), R(O, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FarmNightmareCap:
                return Prototype(kind, R("farm_module_dream_cap", 1, "dream_cap", 1, "restricted_chemical", 1, Elec, 2), R("farm_module_dream_cap", 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.FarmSmuggler:
                return Prototype(kind, R(Eco, 1, "common_chemical", 1, "restricted_chemical", 1, Elec, 2), R(Eco, 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.PrinterCnc:
                return Prototype(kind, R("printer_module_metal", 1, "metal_ingot", 1, "printer_plastic", 1, Elec, 2), R("printer_module_metal", 1, U, 2, Elec, 2));
            case ModuleCrafterChipKind.DnaPrecision:
                return Prototype(kind, R("dna_manipulator_module_positive_filter", 1, "dna_manipulator_module_negative_filter", 1, "plant_dna_disk", 1, Elec, 2), R("dna_manipulator_module_positive_filter", 1, "dna_manipulator_module_negative_filter", 1, U, 1, Elec, 2));
            case ModuleCrafterChipKind.AlarmTransmitter:
                return Prototype(kind, R(P, 1, Elec, 2, "scrap_metal", 1, ModuleCrafterIds.Battery, 1), R(P, 1, U, 1, Elec, 2, ModuleCrafterIds.Battery, 1));
            default:
                return null;
        }
    }

    private static ModuleCrafterRecipe Specialized(ModuleCrafterChipKind kind, ModuleCrafterMaterialNeed[] themed, ModuleCrafterMaterialNeed[] universal)
    {
        return new ModuleCrafterRecipe(kind, ModuleCrafterTier.Specialized, 2, 65, 95, 60, themed, universal);
    }

    private static ModuleCrafterRecipe Precision(ModuleCrafterChipKind kind, ModuleCrafterMaterialNeed[] themed, ModuleCrafterMaterialNeed[] universal)
    {
        return new ModuleCrafterRecipe(kind, ModuleCrafterTier.Precision, 3, 50, 90, 50, themed, universal);
    }

    private static ModuleCrafterRecipe Prototype(ModuleCrafterChipKind kind, ModuleCrafterMaterialNeed[] themed, ModuleCrafterMaterialNeed[] universal)
    {
        return new ModuleCrafterRecipe(kind, ModuleCrafterTier.Prototype, 4, 45, 85, 50, themed, universal);
    }

    private static ModuleCrafterMaterialNeed[] R(params object[] values)
    {
        if (values == null || values.Length % 2 != 0)
        {
            throw new ArgumentException("Material pairs are incomplete.", nameof(values));
        }

        List<ModuleCrafterMaterialNeed> result = new List<ModuleCrafterMaterialNeed>();
        for (int i = 0; i < values.Length; i += 2)
        {
            result.Add(new ModuleCrafterMaterialNeed((string)values[i], (int)values[i + 1]));
        }

        return result.ToArray();
    }

    public static int BaseCatalogPrice(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.FarmNutrifruit => 150,
            ModuleCrafterChipKind.FarmDreamCap => 150,
            ModuleCrafterChipKind.FarmKottonLoom => 200,
            ModuleCrafterChipKind.FarmFillerweed => 115,
            ModuleCrafterChipKind.FarmHydroreed => 105,
            ModuleCrafterChipKind.EvaporatorSolventRecovery => 275,
            ModuleCrafterChipKind.ChemEquipment => 250,
            ModuleCrafterChipKind.FurnaceJunk => 120,
            ModuleCrafterChipKind.PrinterMetal => 300,
            ModuleCrafterChipKind.PrinterMeleeBatch => 250,
            ModuleCrafterChipKind.DnaStabilizer => 500,
            ModuleCrafterChipKind.DnaPositiveFilter => 500,
            ModuleCrafterChipKind.DnaNegativeFilter => 500,
            ModuleCrafterChipKind.FurnaceBlast => 150,
            ModuleCrafterChipKind.FarmNightmareCap => 150,
            ModuleCrafterChipKind.FarmSmuggler => 100,
            ModuleCrafterChipKind.PrinterCnc => 400,
            ModuleCrafterChipKind.DnaPrecision => 1000,
            ModuleCrafterChipKind.AlarmTransmitter => 100,
            _ => 0
        };
    }
}
