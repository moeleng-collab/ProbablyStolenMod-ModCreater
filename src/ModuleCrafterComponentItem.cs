#nullable disable

public static class ModuleCrafterComponentItem
{
    public static GameItem Create()
    {
        GameItem item = ItemDirectory.CreateEmptyItem(ModuleCrafterExpansion.UniversalComponent)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, ModuleCrafterExpansion.UniversalComponent)
            .SetName(ModuleCrafterLocalization.T("component_name"));
        // Keep the native electronic component's 3x2 inventory footprint;
        // transparent margins in the new art must not shrink it to 1x2.
        item.SetShape(GameItem.BuildShapeFromSprite("Items/items_material", "electronic1"));
        item.shortDescription = ModuleCrafterLocalization.T("component_desc");
        item.flavorText = ModuleCrafterLocalization.T("component_flavor");
        item.SetGameItemType("MATERIAL");
        ModHelper.SetSource(item, ModuleCrafterMod.ManifestId, "Module Crafter");
        item.unitValue = ModuleCrafterExpansion.UniversalComponentUnitValue;
        item.mayStackItemFunc = (GameItem first, GameItem second) => first != null && second != null
            && first.identifier == ModuleCrafterExpansion.UniversalComponent
            && second.identifier == ModuleCrafterExpansion.UniversalComponent
            && GeneralHelper.IsItemOwned(first) == GeneralHelper.IsItemOwned(second);
        item.maxSlotAmountItemFunc = (GameItem _, GameInventory _, SlotMarker _) => ModuleCrafterExpansion.UniversalComponentMaxStack;
        item.maxSlotAddItemFunc = (GameItem _, GameInventory _, SlotMarker _) => ModuleCrafterExpansion.UniversalComponentMaxStack;
        return item;
    }
}
