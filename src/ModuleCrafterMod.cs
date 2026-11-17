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
        ModHook.OnModItemDirectoryInit += ModuleCrafterMachine.RegisterItems;
        ModHook.OnModItemDirectoryInit += ModuleCrafterEnhancerMachine.RegisterItems;
        ModHook.OnGameLoadedLate += ModuleCrafterMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhancerMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate += ModuleCrafterDebug.Register;
        ModHook.OnCreateTooltipLate += ModuleCrafterMachine.OnTooltip;
        ModHook.OnCreateTooltipLate += ModuleCrafterEnhancerMachine.OnTooltip;
        ModHook.OnPlaceInventorInventoryItemLate += ModuleCrafterStock.OnPlaceInventorInventory;
        ModHook.OnGenerateCustomerNormal += ModuleCrafterMerchant.OnGenerateCustomer;
        ModHook.OnGameLoadedLate += ModuleCrafterMerchant.OnGameLoaded;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhanceState.RestoreAll;
        ModHook.OnGameLoadedLate += ModuleCrafterEnhanceVisuals.Start;
        ModHook.OnCreateTooltipLate += ModuleCrafterEnhancement.OnTooltip;
        ModHook.OnGenerateCustomerLate += ModuleCrafterEnhanceSources.OnGenerateCustomerLate;
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll();
        ModuleCrafterDebug.Register();
        LogMsg("Enabled");
    }

    public void OnDisable()
    {
        ModuleCrafterEnhanceSources.IsActive = false;
        ModHook.OnModItemDirectoryInit -= ModuleCrafterMachine.RegisterItems;
        ModHook.OnModItemDirectoryInit -= ModuleCrafterEnhancerMachine.RegisterItems;
        ModHook.OnGameLoadedLate -= ModuleCrafterMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhancerMachine.OnGameLoaded;
        ModHook.OnGameLoadedLate -= ModuleCrafterDebug.Register;
        ModHook.OnCreateTooltipLate -= ModuleCrafterMachine.OnTooltip;
        ModHook.OnCreateTooltipLate -= ModuleCrafterEnhancerMachine.OnTooltip;
        ModHook.OnPlaceInventorInventoryItemLate -= ModuleCrafterStock.OnPlaceInventorInventory;
        ModHook.OnGenerateCustomerNormal -= ModuleCrafterMerchant.OnGenerateCustomer;
        ModHook.OnGameLoadedLate -= ModuleCrafterMerchant.OnGameLoaded;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhanceState.RestoreAll;
        ModHook.OnGameLoadedLate -= ModuleCrafterEnhanceVisuals.Start;
        ModHook.OnCreateTooltipLate -= ModuleCrafterEnhancement.OnTooltip;
        ModHook.OnGenerateCustomerLate -= ModuleCrafterEnhanceSources.OnGenerateCustomerLate;
        ModuleCrafterEnhanceState.RestoreUnenhancedForDisable();
        ModuleCrafterDebug.Unregister();
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
