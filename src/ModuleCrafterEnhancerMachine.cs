#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterEnhancerMachine
{
    public const int MaterialsSlotIndex = 0;

    public static void RegisterItems(ModItemDirectory directory)
    {
        directory.Add(ModuleCrafterIds.Enhancer, CreateMachine);
    }

    public static void OnGameLoaded()
    {
        if (PlayerStore.Instance == null)
        {
            return;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (item == null || !item.IsTag(ModuleCrafterIds.EnhancerTag))
            {
                continue;
            }

            MachineHelper.OnLoad(item);
            // Migrate enhancers created from transparent centre samples (7 cells) to the crafter's full 3x3 footprint.
            item.SetShape(new GridShapeBuilder().SetData(ModuleCrafterRules.FullMachineShapeData, ModuleCrafterRules.MachineShapeWidth));
            item.unitValue = ModuleCrafterEnhanceRules.EnhancerUnitValue;
        }
    }

    public static void OnTooltip(RichTextBuilder builder, GameItem item)
    {
        if (item != null && item.IsTag(ModuleCrafterIds.EnhancerTag))
        {
            builder.AddLine(ModuleCrafterLocalization.T("enhancer_tooltip"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
        }
    }

    public static GameItem CreateMachine()
    {
        // A single shared grid holds the target, catalysts, clips and exchange scrap.
        GameGridInventory materials = new GameGridInventory(8, 6);
        materials.mayInventoryAddItemFunc = MayAddMaterial;

        MachineryText preview = new MachineryText(ModuleCrafterLocalization.T("enhance_empty"), 6)
            .SetUppercase(false)
            .SetFillWidth(true)
            .SetMinHeight(54);
        MachineryButton enhanceButton = new MachineryButton(ModuleCrafterLocalization.T("btn_enhance"));
        MachineryButton exchangeButton = new MachineryButton(ModuleCrafterLocalization.T("btn_exchange"));
        MachineryStack controls = MachineryStack.Vertical(preview, enhanceButton, exchangeButton);

        PixelWindow window = MachineHelper.CreateMachineryWindow(
            true,
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_materials"), materials, "materials"),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_control"), controls, "control", styleInventories: false));

        GameItem machine = ItemDirectory.CreateEmptyItem(ModuleCrafterIds.Enhancer)
            .SetContentWindow(window)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, ModuleCrafterIds.Enhancer)
            .SetName(ModuleCrafterLocalization.T("enhancer_name"));

        // Match the crafter's full 3x3 occupancy regardless of transparent pixels at cell centres.
        machine.SetShape(new GridShapeBuilder().SetData(ModuleCrafterRules.FullMachineShapeData, ModuleCrafterRules.MachineShapeWidth));
        machine.SetGameItemType("MACHINE");
        machine.shortDescription = ModuleCrafterLocalization.T("enhancer_desc");
        machine.flavorText = ModuleCrafterLocalization.T("enhancer_flavor");
        machine.unitValue = ModuleCrafterEnhanceRules.EnhancerUnitValue;
        machine.EnableTag("CONTAINER_TAG");
        machine.EnableTag(ModuleCrafterIds.EnhancerTag);

        // Zero drawn power and no battery/progress callbacks: this is an instant anvil.
        MachineryHelper.InitMachinery(machine, 0);

        enhanceButton.SetCallback(() => ModuleCrafterEnhancement.TryEnhance(machine, preview, materials));
        exchangeButton.SetCallback(() => ModuleCrafterEnhancement.TryExchangeScrap(preview, materials));
        Action refresh = () => ModuleCrafterEnhancement.RefreshPreview(preview, materials);
        HookInventoryChanged(materials, refresh);
        refresh();
        return machine;
    }

    private static bool MayAddMaterial(GameItem item, GameInventory inventory)
    {
        if (item == null)
        {
            return false;
        }

        if (ModuleCrafterEnhanceState.IsIntactSupportedModule(item))
        {
            GameItem existing = ModuleCrafterEnhancement.FindTarget(inventory);
            return existing == null || existing == item;
        }

        return item.identifier == ModuleCrafterIds.EnhanceCatalyst
            || item.identifier == ModuleCrafterIds.SafeguardClip
            || item.identifier == ModuleCrafterIds.Scrap;
    }

    private static void HookInventoryChanged(GameInventory inventory, Action refresh)
    {
        if (inventory == null || refresh == null)
        {
            return;
        }

        Action<GameItem, GameInventory, SlotMarker> previousAdd = inventory.onSlotAddItemEndFunc;
        inventory.onSlotAddItemEndFunc = delegate(GameItem item, GameInventory inv, SlotMarker slot)
        {
            previousAdd?.Invoke(item, inv, slot);
            refresh();
        };

        Action<GameItem, GameInventory, SlotMarker> previousRemove = inventory.onSlotRemoveItemEndFunc;
        inventory.onSlotRemoveItemEndFunc = delegate(GameItem item, GameInventory inv, SlotMarker slot)
        {
            previousRemove?.Invoke(item, inv, slot);
            refresh();
        };
    }
}
