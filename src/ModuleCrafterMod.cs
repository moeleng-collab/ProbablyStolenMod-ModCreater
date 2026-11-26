#nullable disable
using HarmonyLib;

public sealed class ModuleCrafterMod : IMod
{
    public const string ManifestId = "module_crafter";
    public const string HarmonyId = "module_crafter";

    internal static ModLog Log;
    internal static string ModVersion = "";
    private Harmony _harmony;

    public void Init(ModManifest manifest)
    {
        Log = new ModLog(manifest);
        ModVersion = manifest != null ? (manifest.ModVersion ?? "") : "";
    }

    public void OnEnable()
    {
        ModuleCrafterEnhanceSources.IsActive = true;
        ModuleCrafterScenario.Register();
        ModHook.OnModItemDirectoryInit += ModuleCrafterMachine.RegisterItems;
        ModHook.OnModItemDirectoryInit += ModuleCrafterEnhancerMachine.RegisterItems;
        ModHook.OnGameLoadedLate += ModuleCrafterNetworkUpgrades.Register;
        ModHook.OnGameLoadedLate += ModuleCrafterMigration.Run;
        ModHook.OnGameLoadedLate += ModuleCrafterMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhancerMachine.OnGameLoaded;
#if MC_DEBUG
        ModHook.OnGameLoadedLate += ModuleCrafterDebug.Register;
#endif
        ModHook.OnCreateTooltipLate += ModuleCrafterMachine.OnTooltip;
        ModHook.OnCreateTooltipLate += ModuleCrafterEnhancerMachine.OnTooltip;
        ModHook.OnPlaceInventorInventoryItemLate += ModuleCrafterStock.OnPlaceInventorInventory;
        ModHook.OnGenerateCustomerNormal += ModuleCrafterMerchant.OnGenerateCustomer;
        ModHook.OnGameLoadedLate += ModuleCrafterMerchant.OnGameLoaded;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhanceState.RestoreAll;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhanceVisuals.Start;
        ModHook.OnCreateTooltipLate += ModuleCrafterEnhancement.OnTooltip;
        ModHook.OnGenerateCustomerLate += ModuleCrafterEnhanceSources.OnGenerateCustomerLate;
        ModHook.OnGenerateCustomerLate += ModuleCrafterExpansionStock.OnGenerateCustomerLate;
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll();
#if MC_DEBUG
        ModuleCrafterDebug.Register();
#endif
        LogMsg("Enabled");
    }

    public void OnDisable()
    {
        ModuleCrafterEnhanceSources.IsActive = false;
        ModuleCrafterScenario.Unregister();
        ModuleCrafterNetworkUpgrades.Unregister();
        ModHook.OnModItemDirectoryInit -= ModuleCrafterMachine.RegisterItems;
        ModHook.OnModItemDirectoryInit -= ModuleCrafterEnhancerMachine.RegisterItems;
        ModHook.OnGameLoadedLate -= ModuleCrafterMigration.Run;
        ModHook.OnGameLoadedLate -= ModuleCrafterMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhancerMachine.OnGameLoaded;
#if MC_DEBUG
        ModHook.OnGameLoadedLate -= ModuleCrafterDebug.Register;
#endif
        ModHook.OnCreateTooltipLate -= ModuleCrafterMachine.OnTooltip;
        ModHook.OnCreateTooltipLate -= ModuleCrafterEnhancerMachine.OnTooltip;
        ModHook.OnPlaceInventorInventoryItemLate -= ModuleCrafterStock.OnPlaceInventorInventory;
        ModHook.OnGenerateCustomerNormal -= ModuleCrafterMerchant.OnGenerateCustomer;
        ModHook.OnGameLoadedLate -= ModuleCrafterMerchant.OnGameLoaded;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhanceState.RestoreAll;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhanceVisuals.Start;
        ModHook.OnCreateTooltipLate -= ModuleCrafterEnhancement.OnTooltip;
        ModHook.OnGenerateCustomerLate -= ModuleCrafterEnhanceSources.OnGenerateCustomerLate;
        ModHook.OnGenerateCustomerLate -= ModuleCrafterExpansionStock.OnGenerateCustomerLate;
        ModuleCrafterExpansionStock.Reset();
        ModuleCrafterSpecialEffectAdapters.Reset();
        ModuleCrafterEnhanceState.RestoreUnenhancedForDisable();
#if MC_DEBUG
        ModuleCrafterDebug.Unregister();
#endif
        if (_harmony != null)
        {
            _harmony.UnpatchAll(HarmonyId);
            _harmony = null;
        }

        LogMsg("Disabled");
    }

    internal static void LogMsg(object message)
    {
        Log?.Log(message);
    }
}
