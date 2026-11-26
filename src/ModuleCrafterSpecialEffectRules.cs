#nullable disable

public enum ModuleCrafterDiskRetention
{
    Never = 0,
    FiftyPercent = 1,
    Always = 2
}

public enum ModuleCrafterContrabandScope
{
    GrowingSeeds = 0,
    HarvestedCrops = 1,
    AllOutputs = 2
}

public readonly struct ModuleCrafterSpecialEffectState
{
    public ModuleCrafterSpecialEffectState(
        int outputBonus = 0,
        int yieldBonus = 0,
        int qualityBonus = 0,
        int workloadReductionPercent = 0,
        int recoveryPercent = 0,
        bool keepTarget = false,
        ModuleCrafterDiskRetention keepDisk = ModuleCrafterDiskRetention.Never,
        ModuleCrafterContrabandScope contrabandScope = ModuleCrafterContrabandScope.GrowingSeeds,
        bool removePurityPenalty = false,
        int extraTraitCount = 0)
    {
        OutputBonus = outputBonus;
        YieldBonus = yieldBonus;
        QualityBonus = qualityBonus;
        WorkloadReductionPercent = workloadReductionPercent;
        RecoveryPercent = recoveryPercent;
        KeepTarget = keepTarget;
        KeepDisk = keepDisk;
        ContrabandScope = contrabandScope;
        RemovePurityPenalty = removePurityPenalty;
        ExtraTraitCount = extraTraitCount;
    }

    public int OutputBonus { get; }
    public int YieldBonus { get; }
    public int QualityBonus { get; }
    public int WorkloadReductionPercent { get; }
    public int RecoveryPercent { get; }
    public bool KeepTarget { get; }
    public ModuleCrafterDiskRetention KeepDisk { get; }
    public ModuleCrafterContrabandScope ContrabandScope { get; }
    public bool RemovePurityPenalty { get; }
    public int ExtraTraitCount { get; }
}

public static class ModuleCrafterSpecialEffectRules
{
    public static ModuleCrafterSpecialEffectState Evaluate(ModuleCrafterChipKind kind, int level)
    {
        int stage = level >= 12 ? 2 : level >= 10 ? 1 : 0;
        return kind switch
        {
            ModuleCrafterChipKind.FarmNutrifruit => new ModuleCrafterSpecialEffectState(yieldBonus: stage == 0 ? 1 : stage == 1 ? 2 : 3),
            ModuleCrafterChipKind.FarmDreamCap => new ModuleCrafterSpecialEffectState(yieldBonus: stage == 0 ? 2 : stage == 1 ? 3 : 4),
            ModuleCrafterChipKind.FarmKottonLoom => new ModuleCrafterSpecialEffectState(outputBonus: stage),
            ModuleCrafterChipKind.FarmFillerweed => new ModuleCrafterSpecialEffectState(yieldBonus: stage == 0 ? 4 : stage == 1 ? 6 : 8),
            ModuleCrafterChipKind.FarmHydroreed => new ModuleCrafterSpecialEffectState(yieldBonus: stage == 0 ? 1 : stage == 1 ? 2 : 3),
            ModuleCrafterChipKind.FarmSmuggler => new ModuleCrafterSpecialEffectState(contrabandScope: (ModuleCrafterContrabandScope)stage),
            ModuleCrafterChipKind.FarmNightmareCap => new ModuleCrafterSpecialEffectState(outputBonus: stage),
            ModuleCrafterChipKind.EvaporatorSolventRecovery => new ModuleCrafterSpecialEffectState(recoveryPercent: stage == 0 ? 0 : stage == 1 ? 10 : 20),
            ModuleCrafterChipKind.DnaStabilizer => new ModuleCrafterSpecialEffectState(keepTarget: true, keepDisk: stage == 0 ? ModuleCrafterDiskRetention.Never : stage == 1 ? ModuleCrafterDiskRetention.FiftyPercent : ModuleCrafterDiskRetention.Always),
            ModuleCrafterChipKind.DnaPositiveFilter => new ModuleCrafterSpecialEffectState(extraTraitCount: stage),
            ModuleCrafterChipKind.DnaNegativeFilter => new ModuleCrafterSpecialEffectState(extraTraitCount: stage),
            ModuleCrafterChipKind.DnaPrecision => new ModuleCrafterSpecialEffectState(keepTarget: stage >= 1, keepDisk: stage == 2 ? ModuleCrafterDiskRetention.Always : ModuleCrafterDiskRetention.Never),
            ModuleCrafterChipKind.PrinterMetal => new ModuleCrafterSpecialEffectState(qualityBonus: stage == 0 ? 0 : stage == 1 ? 10 : 20),
            ModuleCrafterChipKind.PrinterCnc => new ModuleCrafterSpecialEffectState(workloadReductionPercent: stage == 0 ? 0 : stage == 1 ? 10 : 20),
            ModuleCrafterChipKind.PrinterMeleeBatch => new ModuleCrafterSpecialEffectState(outputBonus: 1 + stage),
            ModuleCrafterChipKind.AlarmTransmitter => new ModuleCrafterSpecialEffectState(outputBonus: 1 + stage),
            ModuleCrafterChipKind.FurnaceJunk => new ModuleCrafterSpecialEffectState(outputBonus: stage == 0 ? 4 : stage == 1 ? 5 : 6),
            ModuleCrafterChipKind.FurnaceBlast => new ModuleCrafterSpecialEffectState(outputBonus: stage == 0 ? 2 : 3, removePurityPenalty: stage == 2),
            ModuleCrafterChipKind.ChemEquipment => new ModuleCrafterSpecialEffectState(qualityBonus: stage == 0 ? 0 : stage == 1 ? 10 : 20),
            _ => default
        };
    }

    public static int StageForLevel(int level)
    {
        return level >= 12 ? 2 : level >= 10 ? 1 : 0;
    }
}
