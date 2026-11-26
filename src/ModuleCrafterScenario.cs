#nullable disable
using System;
using HarmonyLib;

public static class ModuleCrafterScenario
{
    public const string ScenarioId = "module_crafter_manufacturer";
    public const int SuccessBonusPercent = 10;
    public const int ScavAttemptBonus = 2;

    public static void Register()
    {
        StartingScenario scenario = new StartingScenario
        {
            id = ScenarioId,
            startType = NewGameData.StartType.MODDED,
            source = ModuleCrafterMod.ManifestId,
            iconKey = ModuleCrafterMod.ManifestId + ":module_crafter_manufacturer",
            titleProvider = () => ModuleCrafterLocalization.T("scenario_name"),
            descriptionProvider = () => ModuleCrafterLocalization.T("scenario_desc"),
            tagProvider = () => ModuleCrafterLocalization.T("scenario_advanced"),
            tagTone = StartingScenario.TagTone.Warning,
            skipIntro = true,
            comingSoon = false,
            hidden = false,
            onStart = GrantStartingItems
        };
        scenario.startWithProviders.Add(() => ModuleCrafterLocalization.T("scenario_gear_machine"));
        scenario.startWithProviders.Add(() => ModuleCrafterLocalization.T("scenario_gear_chip"));
        scenario.startWithProviders.Add(() => ModuleCrafterLocalization.T("scenario_gear_scav", ScavAttemptBonus));
        scenario.startWithProviders.Add(() => ModuleCrafterLocalization.T("scenario_gear_craft", SuccessBonusPercent));
        scenario.startWithProviders.Add(() => ModuleCrafterLocalization.T("scenario_gear_enhance", SuccessBonusPercent));
        StartingScenario.Register(scenario);
    }

    public static void Unregister()
    {
        StartingScenario.RemoveMod(ModuleCrafterMod.ManifestId);
    }

    public static bool IsManufacturer()
    {
        if (PlayerStore.Instance == null)
        {
            return false;
        }

        string id = StartingScenario.ResolveId(PlayerStore.Instance.startType, PlayerStore.Instance.startScenarioId);
        return id == ScenarioId;
    }

    public static int ApplySuccessBonus(int basePercent)
    {
        int value = basePercent + (IsManufacturer() ? SuccessBonusPercent : 0);
        return Math.Clamp(value, 0, 100);
    }

    private static void GrantStartingItems()
    {
        try
        {
            GrantItem(DirectoryMaster.Item(ModuleCrafterIds.Machine));
            GrantItem(DirectoryMaster.Item(ModuleCrafterIds.Chip(ModuleCrafterChipKind.Performance)));
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Module manufacturer starting items failed: " + ex);
        }
    }

    private static void GrantItem(GameItem item)
    {
        if (item == null || EmporiumEntry.Instance == null)
        {
            throw new InvalidOperationException("Starting item or emporium is unavailable.");
        }

        GeneralHelper.SetItemOwned(item, true);
        if (GraphUtils.TryAcceptAll(EmporiumEntry.Instance.invElement, item) == 0
            || GraphUtils.TryAcceptAll(EmporiumEntry.Instance.frontInvinvElement, item) == 0
            || GraphUtils.TryAcceptAll(EmporiumEntry.Instance.backInvinvElement, item) == 0)
        {
            return;
        }

        throw new InvalidOperationException("No inventory accepts starting item " + item.identifier + ".");
    }
}

[HarmonyPatch(typeof(ScavHelper), nameof(ScavHelper.GetMaxScavAttempts))]
internal static class ModuleCrafterScenarioScavPatch
{
    private static void Postfix(ref int __result)
    {
        if (!StartingPerk.IsMaimed() && ModuleCrafterScenario.IsManufacturer())
        {
            __result = Math.Max(0, __result + ModuleCrafterScenario.ScavAttemptBonus);
        }
    }
}
