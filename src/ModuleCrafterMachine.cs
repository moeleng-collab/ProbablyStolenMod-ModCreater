#nullable disable
using System;
using System.Collections.Generic;

public static class ModuleCrafterMachine
{
    public const int BatterySlotIndex = 0;
    public const int ModuleSlotIndex = 1;
    public const int ChipSlotIndex = 2;
    public const int MaterialsSlotIndex = 3;
    public const int OutputSlotIndex = 4;
    public const int HeldSlotIndex = 5;

    public const string ForceResultTag = "MODULE_CRAFTER_FORCE_TAG";
    public const string KindTag = "MODULE_CRAFTER_KIND_TAG";
    public const string SuccessTag = "MODULE_CRAFTER_SUCCESS_TAG";
    public const string LockedInputTag = "MODULE_CRAFTER_LOCKED_INPUT_TAG";
    public const string RefundTag = "MODULE_CRAFTER_REFUND_TAG";

    public static void RegisterItems(ModItemDirectory directory)
    {
        directory.Add(ModuleCrafterIds.Machine, CreateMachine);
        foreach (ModuleCrafterChipKind kind in (ModuleCrafterChipKind[])Enum.GetValues(typeof(ModuleCrafterChipKind)))
        {
            ModuleCrafterChipKind captured = kind;
            directory.Add(ModuleCrafterIds.Chip(captured), () => CreateChip(captured));
        }

        directory.Add(ModuleCrafterIds.RefundTicket, CreateRefundTicket);
        directory.Add(ModuleCrafterIds.RefundCore, CreateRefundCore);
    }

    public static void OnGameLoaded()
    {
        if (PlayerStore.Instance == null)
        {
            return;
        }

        foreach (GameItem item in PlayerStore.Instance.FindAllItem())
        {
            if (item == null)
            {
                continue;
            }

            ApplyCatalogValue(item);
            if (!item.IsTag(ModuleCrafterIds.MachineTag))
            {
                continue;
            }

            MachineHelper.OnLoad(item);
            foreach (GameItem child in SlotItems(MachineHelper.GetSlot(item, ChipSlotIndex)))
            {
                GeneralHelper.LoadLockItem(child);
            }

            foreach (GameItem child in SlotItems(MachineHelper.GetSlot(item, MaterialsSlotIndex)))
            {
                GeneralHelper.LoadLockItem(child);
            }

            foreach (GameItem child in SlotItems(MachineHelper.GetSlot(item, HeldSlotIndex)))
            {
                GeneralHelper.LoadLockItem(child);
            }
        }
    }

    private static void ApplyCatalogValue(GameItem item)
    {
        if (item.IsTag(ModuleCrafterIds.MachineTag) || item.identifier == ModuleCrafterIds.Machine)
        {
            int stage = item.GetTagReadonly("MODULE_UPGRADE_STAGE_INT").GetInt();
            item.unitValue = ModuleCrafterRules.MachineCatalogValue(stage);
            return;
        }

        if (ModuleCrafterIds.TryGetChipKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            item.unitValue = ModuleCrafterRules.ChipUnitValue(kind);
            return;
        }

        if (item.identifier == ModuleCrafterIds.RefundTicket)
        {
            item.unitValue = ModuleCrafterRules.RefundTicketUnitValue;
        }
        else if (item.identifier == ModuleCrafterIds.RefundCore)
        {
            item.unitValue = ModuleCrafterRules.RefundCoreUnitValue;
        }
    }

    public static void OnTooltip(RichTextBuilder builder, GameItem item)
    {
        if (item == null)
        {
            return;
        }

        if (item.IsTag(ModuleCrafterIds.MachineTag))
        {
            if (IsWorking(item))
            {
                (int current, int target) = MachineProgressHelper.GetProgress(item);
                builder.AddLine(ModuleCrafterLocalization.T("tooltip_working", current, target), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
            }
            else
            {
                GameSlotInventory output = MachineHelper.GetSlot(item, OutputSlotIndex) as GameSlotInventory;
                if (output != null && output.childItem != null)
                {
                    builder.AddLine(ModuleCrafterLocalization.T("tooltip_finished"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
                }
                else
                {
                    builder.AddLine(ModuleCrafterLocalization.T("tooltip_ready"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
                }
            }
        }
        else if (ModuleCrafterIds.TryGetChipKind(item.identifier, out ModuleCrafterChipKind kind))
        {
            builder.AddLine(ModuleCrafterLocalization.T("chip_desc"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
            int work = item.IsTag("PROGRESS_ITEM_TARGET_TAG")
                ? item.GetTagReadonly("PROGRESS_ITEM_TARGET_TAG").GetInt()
                : ModuleCrafterRules.DefaultBaseProcessingSpeed * ModuleCrafterRules.GetRecipe(kind).BaseNights;
            builder.AddLine(LocHelper.GetLocalizedMechanic("mech_machine_required_work") + " " + work, ignoreEmpty: true, RenderHandler.ColorPalette.LightGreen);
            builder.AddLine(ModuleCrafterLocalization.FormatRequirements(kind), ignoreEmpty: true, RenderHandler.ColorPalette.LightYellow);
        }
        else if (item.identifier == ModuleCrafterIds.RefundTicket)
        {
            builder.AddLine(ModuleCrafterLocalization.T("ticket_desc"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
        }
        else if (item.identifier == ModuleCrafterIds.RefundCore)
        {
            builder.AddLine(ModuleCrafterLocalization.T("core_desc"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
        }
    }

    public static GameItem CreateMachine()
    {
        GameSlotInventory battery = new GameSlotInventory(2, 2);
        GameGridInventory modules = new GameGridInventory("10000010001000001110000010001000001", 7);
        GameSlotInventory chip = new GameSlotInventory(2, 2);
        // 8x6 holds uncapped materials (2 modules + 4 electronics) plus up to 5 extra electronics.
        GameGridInventory materials = new GameGridInventory(8, 6);
        GameSlotInventory output = new GameSlotInventory(4, 4);
        GameGridInventory held = new GameGridInventory(8, 6);
        MachineryText preview = new MachineryText(ModuleCrafterLocalization.T("preview_empty"), 6)
            .SetUppercase(false)
            .SetFillWidth(true)
            .SetMinHeight(18);
        MachineryButton startBtn = new MachineryButton(ModuleCrafterLocalization.T("btn_start"));
        MachineryButton cancelBtn = new MachineryButton(ModuleCrafterLocalization.T("btn_cancel"));
        MachineryStack controls = MachineryStack.Vertical(preview, startBtn, cancelBtn);

        chip.mayInventoryAddItemFunc = (GameItem item, GameInventory inventory) =>
            item != null && ModuleCrafterIds.TryGetChipKind(item.identifier, out ModuleCrafterChipKind kind);
        materials.mayInventoryAddItemFunc = (GameItem item, GameInventory inventory) =>
            item != null && ModuleCrafterRules.IsCraftMaterial(item.identifier);
        output.mayInventoryAddItemFunc = (GameItem _, GameInventory _) => false;
        held.mayInventoryAddItemFunc = (GameItem _, GameInventory _) => false;

        PixelWindow window = MachineHelper.CreateMachineryWindow(
            true,
            MachineHelper.PowerPanel(battery),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_module"), modules, "module"),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_chip"), chip, "chip"),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_materials"), materials, "materials"),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_output"), output, "output"),
            MachineHelper.Panel(ModuleCrafterLocalization.T("label_control"), controls, "control", styleInventories: false),
            MachineHelper.Panel("", held, "held").SetHidden(true));

        GameItem machine = ItemDirectory.CreateEmptyItem(ModuleCrafterIds.Machine)
            .SetContentWindow(window)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, ModuleCrafterIds.Machine)
            .SetName(ModuleCrafterLocalization.T("machine_name"));

        machine.SetGameItemType("MACHINE");
        machine.shortDescription = ModuleCrafterLocalization.T("machine_desc");
        machine.flavorText = ModuleCrafterLocalization.T("machine_flavor");
        machine.unitValue = ModuleCrafterRules.MachineUnitValue;
        machine.EnableTag("CONTAINER_TAG");
        machine.EnableTag("manufacture");
        machine.EnableTag("STANDARD_MACHINE_TAG");
        machine.EnableTag(ModuleCrafterIds.MachineTag);

        MachineryHelper.InitMachinery(machine, 10);
        MachineProgressHelper.InitProgressTypeMachine(machine, ModuleCrafterRules.DefaultBaseProcessingSpeed, useDefaultTooltip: false);
        machine.ModifyTag("MACHINE_QUALITY_RATIO_INT", delegate(TagState state)
        {
            state.Enable().SetInt(1);
        });

        MachineHelper.SetupBatterySlot(battery, machine);
        MachineHelper.SetupModuleBay(modules, machine, () => RefreshPreview(machine, preview, chip, materials, output));

        startBtn.SetCallback(() => TryStart(machine, preview, chip, materials, output));
        cancelBtn.SetCallback(() => TryCancel(machine, preview, chip, materials, output));

        Action refresh = () => RefreshPreview(machine, preview, chip, materials, output);
        HookInventoryChanged(battery, refresh);
        HookInventoryChanged(chip, refresh);
        HookInventoryChanged(materials, refresh);
        HookInventoryChanged(output, refresh);

        machine.mayItemTargetItemFunc = (GameItem incoming, GameItem target) =>
            target == machine && ModuleCrafterRules.ShouldAcceptMachineTarget(
                MayRouteOntoMachine(incoming, machine),
                incoming?.mayItemTargetItemFunc?.InvokeAllReduce(incoming, machine, InvokeFuncExtensions.BoolReduceOR));
        machine.canItemTargetItemFunc = (GameItem incoming, GameItem target) =>
            target == machine && ModuleCrafterRules.ShouldAcceptMachineTarget(
                MayRouteOntoMachine(incoming, machine),
                incoming?.canItemTargetItemFunc?.InvokeAllReduce(incoming, machine, InvokeFuncExtensions.BoolReduceOR));
        machine.onItemTargetItemFunc = delegate(GameItem incoming, GameItem target)
        {
            if (target == machine)
            {
                RouteOntoMachine(incoming, machine);
            }
        };

        machine.onCycleEndSlotItemFunc = delegate(GameItem item, GameInventory parent, SlotMarker slot)
        {
            OnNight(item, preview, chip, materials, output, modules);
        };

        RefreshPreview(machine, preview, chip, materials, output);
        return machine;
    }

    public static GameItem CreateChip(ModuleCrafterChipKind kind)
    {
        string id = ModuleCrafterIds.Chip(kind);
        GameItem chip = ItemDirectory.CreateEmptyItem(id)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, id)
            .SetName(ModuleCrafterLocalization.T("chip_name", ModuleCrafterLocalization.KindName(kind)));
        chip.EnableTag(ModuleCrafterIds.ChipTag);
        chip.EnableTag("IMPORTANT_TAG");
        chip.canUseOutsideCombat = true;
        chip.unitValue = ModuleCrafterRules.ChipUnitValue(kind);
        ModuleCrafterRecipe recipe = ModuleCrafterRules.GetRecipe(kind);
        MachineProgressHelper.InitProgressSourceItem(
            chip,
            ModuleCrafterRules.DefaultBaseProcessingSpeed * recipe.BaseNights,
            useDefaultTooltip: false);
        if (kind == ModuleCrafterChipKind.NeuralCapped || kind == ModuleCrafterChipKind.NeuralUncapped)
        {
            ContrabandHelper.InitContrabandItem(chip);
        }

        return chip;
    }

    public static GameItem CreateRefundTicket()
    {
        return CreateRefundItem(
            ModuleCrafterIds.RefundTicket,
            "ticket_name",
            "ticket_desc",
            "ticket_flavor",
            ModuleCrafterRules.RefundTicketUnitValue);
    }

    public static GameItem CreateRefundCore()
    {
        return CreateRefundItem(
            ModuleCrafterIds.RefundCore,
            "core_name",
            "core_desc",
            "core_flavor",
            ModuleCrafterRules.RefundCoreUnitValue);
    }

    private static GameItem CreateRefundItem(string id, string nameKey, string descKey, string flavorKey, int unitValue)
    {
        GameItem item = ItemDirectory.CreateEmptyItem(id)
            .SetSpriteAndShapeFromMod(ModuleCrafterMod.ManifestId, id)
            .SetName(ModuleCrafterLocalization.T(nameKey));
        item.shortDescription = ModuleCrafterLocalization.T(descKey);
        item.flavorText = ModuleCrafterLocalization.T(flavorKey);
        item.SetGameItemType("MATERIAL");
        item.EnableTag("IMPORTANT_TAG");
        item.unitValue = unitValue;
        return item;
    }

    public static void FinishJob(GameItem machine, bool skipNightCheck = false)
    {
        GameSlotInventory chip = MachineHelper.GetSlot(machine, ChipSlotIndex) as GameSlotInventory;
        GameGridInventory materials = MachineHelper.GetSlot(machine, MaterialsSlotIndex) as GameGridInventory;
        GameSlotInventory output = MachineHelper.GetSlot(machine, OutputSlotIndex) as GameSlotInventory;
        GameGridInventory modules = MachineHelper.GetSlot(machine, ModuleSlotIndex) as GameGridInventory;
        FinishJob(machine, null, chip, materials, output, modules, skipNightCheck);
    }

    private static void TryStart(
        GameItem machine,
        MachineryText preview,
        GameSlotInventory chip,
        GameGridInventory materials,
        GameSlotInventory output)
    {
        if (machine == null || IsWorking(machine))
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        GameItem chipItem = chip != null ? chip.childItem : null;
        if (chipItem == null || !ModuleCrafterIds.TryGetChipKind(chipItem.identifier, out ModuleCrafterChipKind kind))
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        if (output != null && output.childItem != null)
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        ModuleCrafterMaterialPlan plan = PlanFrom(kind, materials);
        bool powered = MachineHelper.CanPower(machine);
        if (!ModuleCrafterRules.CanStart(plan.HasRequired, powered))
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        ModuleCrafterRecipe recipe = ModuleCrafterRules.GetRecipe(kind);
        int quality = ModuleHelper.GetAjustedQuality(machine);
        int success = ModuleCrafterRules.ComputeSuccessPercent(kind, quality, plan.ExtraElectronics, plan.UsedIntactHousing);
        MachineProgressHelper.InitProgressSourceItem(
            chipItem,
            ModuleCrafterRules.DefaultBaseProcessingSpeed * recipe.BaseNights,
            useDefaultTooltip: false);
        MachineProgressHelper.StartProgressTypeMachine(machine, chipItem);

        GeneralHelper.LockItem(chipItem);
        LockPlannedMaterials(materials, plan);
        LockRefundItems(materials, plan);
        StashPlannedMaterials(machine, materials, plan);
        MachineHelper.LockModuleInv(machine);

        SetString(machine, KindTag, kind.ToString());
        SetInt(machine, SuccessTag, success);
        SetString(machine, RefundTag, plan.RefundKind.ToString());
        if (ModuleCrafterRules.ShouldConsumeTicketOnStart(plan.RefundKind))
        {
            ConsumeOneTicket(materials);
        }

        RefreshPreview(machine, preview, chip, materials, output);
    }

    private static void TryCancel(
        GameItem machine,
        MachineryText preview,
        GameSlotInventory chip,
        GameGridInventory materials,
        GameSlotInventory output)
    {
        if (machine == null || !IsWorking(machine))
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        ReturnHeldMaterials(machine, materials);
        UnlockJob(machine, chip, materials);
        MachineProgressHelper.FinishProgressTypeMachine(machine);
        ClearTag(machine, KindTag);
        ClearTag(machine, SuccessTag);
        ClearTag(machine, RefundTag);
        RefreshPreview(machine, preview, chip, materials, output);
    }

    private static void OnNight(
        GameItem machine,
        MachineryText preview,
        GameSlotInventory chip,
        GameGridInventory materials,
        GameSlotInventory output,
        GameGridInventory modules)
    {
        if (machine == null || !IsWorking(machine))
        {
            return;
        }

        if (output != null && output.childItem != null)
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        GameSlotInventory battery = MachineHelper.GetBatterySlot(machine);
        if (!MachineHelper.TryDrawCyclePower(machine, battery))
        {
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        MachineProgressHelper.ContinueProgressTypeMachine(machine);
        MachineHelper.OnMachineActioned(machine, modules);
        if (MachineProgressHelper.IsProgressTypeMachineFinished(machine))
        {
            FinishJob(machine, preview, chip, materials, output, modules, skipNightCheck: false);
        }
        else
        {
            RefreshPreview(machine, preview, chip, materials, output);
        }
    }

    private static void FinishJob(
        GameItem machine,
        MachineryText preview,
        GameSlotInventory chip,
        GameGridInventory materials,
        GameSlotInventory output,
        GameGridInventory modules,
        bool skipNightCheck)
    {
        if (machine == null)
        {
            return;
        }

        if (output != null && output.childItem != null)
        {
            ModuleCrafterMod.LogMsg("Finish skipped: output is occupied.");
            RefreshPreview(machine, preview, chip, materials, output);
            return;
        }

        ModuleCrafterChipKind kind = ReadKind(machine, chip);
        GameGridInventory held = GetHeld(machine);
        int success;
        if (machine.IsTag(SuccessTag))
        {
            success = machine.GetTagReadonly(SuccessTag).GetInt();
        }
        else
        {
            int quality = ModuleHelper.GetAjustedQuality(machine);
            ModuleCrafterMaterialPlan plan = PlanFrom(kind, materials);
            if (!plan.HasRequired && held != null)
            {
                plan = PlanFrom(kind, held);
            }

            success = ModuleCrafterRules.ComputeSuccessPercent(kind, quality, plan.ExtraElectronics, plan.UsedIntactHousing);
            ModuleCrafterMod.LogMsg("Success tag missing at finish; recomputed " + success + "%.");
        }

        bool passed;
        int roll = -1;
        if (machine.IsTag(ForceResultTag))
        {
            string force = machine.GetTagReadonly(ForceResultTag).GetString();
            passed = string.Equals(force, "success", StringComparison.OrdinalIgnoreCase);
            ClearTag(machine, ForceResultTag);
        }
        else
        {
            passed = ModuleCrafterRules.RollSuccess(success, NextExclusive, out roll);
        }

        ModuleCrafterMod.LogMsg("Craft roll " + roll + " vs " + success + "% => " + (passed ? "success" : "fail") + ".");

        ModuleCrafterRefundKind refundKind = ReadRefundKind(machine);
        bool refundMaterials = ModuleCrafterRules.ShouldRefundMaterials(refundKind, !passed);
        if (passed)
        {
            string outputId = ModuleCrafterIds.OutputModule(kind);
            PlaceCraftResult(output, outputId, kind, applyShape: true);
            ConsumeLockedMaterials(held);
            ConsumeLockedMaterials(materials);
        }
        else if (refundMaterials)
        {
            ReturnHeldMaterials(machine, materials);
            ModuleCrafterMod.LogMsg("Craft failed with refund " + refundKind + "; materials returned.");
        }
        else
        {
            string outputId = ModuleCrafterRules.RollFailure(kind, NextExclusive) == ModuleCrafterFailureOutput.Corrupt
                ? ModuleCrafterIds.Corrupt
                : ModuleCrafterIds.Ruined;
            PlaceCraftResult(output, outputId, kind, applyShape: false);
            ConsumeLockedMaterials(held);
            ConsumeLockedMaterials(materials);
        }

        AddCompletionNightLog(kind, passed, refundMaterials, skipNightCheck);

        UnlockJob(machine, chip, materials);
        MachineProgressHelper.FinishProgressTypeMachine(machine);
        ClearTag(machine, KindTag);
        ClearTag(machine, SuccessTag);
        ClearTag(machine, RefundTag);
        MachineHelper.OnMachineActioned(machine, modules);
        RefreshPreview(machine, preview, chip, materials, output);
        if (preview != null && roll >= 0)
        {
            string resultKey = passed
                ? "preview_result_success"
                : (refundMaterials ? "preview_result_fail_refund" : "preview_result_fail");
            preview.SetText(ModuleCrafterLocalization.T(resultKey, roll, success));
        }
    }

    private static void PlaceCraftResult(GameSlotInventory output, string outputId, ModuleCrafterChipKind kind, bool applyShape)
    {
        GameItem result = DirectoryMaster.Item(outputId);
        if (result == null)
        {
            ModuleCrafterMod.LogMsg("Could not create result " + outputId + ".");
            return;
        }

        GeneralHelper.SetItemOwned(result, own: true);
        if (applyShape)
        {
            ApplyVanillaRandomShape(kind, result);
            ApplyCraftedModuleEffects(result);
        }

        if (outputId == ModuleCrafterIds.NeuralCapped || outputId == ModuleCrafterIds.NeuralUncapped)
        {
            ContrabandHelper.InitContrabandItem(result);
        }

        if (!TryPlaceOutput(output, result))
        {
            GraphUtils.EmporiumTryAdd(result);
            ModuleCrafterMod.LogMsg("Output could not accept the result; sent to shop inventory.");
        }
    }

    private static void RefreshPreview(
        GameItem machine,
        MachineryText preview,
        GameSlotInventory chip,
        GameGridInventory materials,
        GameSlotInventory output)
    {
        if (preview == null || machine == null)
        {
            return;
        }

        if (IsWorking(machine))
        {
            if (output != null && output.childItem != null)
            {
                preview.SetText(ModuleCrafterLocalization.T("preview_paused_output"));
                return;
            }

            if (!MachineHelper.CanPower(machine))
            {
                preview.SetText(ModuleCrafterLocalization.T("preview_paused_power"));
                return;
            }

            (int current, int target) = MachineProgressHelper.GetProgress(machine);
            int success = machine.IsTag(SuccessTag) ? machine.GetTagReadonly(SuccessTag).GetInt() : 0;
            preview.SetText(ModuleCrafterLocalization.T("preview_working", current, target, success) + RefundPreviewText(ReadRefundKind(machine)));
            return;
        }

        GameItem chipItem = chip != null ? chip.childItem : null;
        if (chipItem == null || !ModuleCrafterIds.TryGetChipKind(chipItem.identifier, out ModuleCrafterChipKind kind))
        {
            preview.SetText(ModuleCrafterLocalization.T("preview_empty"));
            return;
        }

        if (output != null && output.childItem != null)
        {
            preview.SetText(ModuleCrafterLocalization.T("preview_output_full"));
            return;
        }

        ModuleCrafterMaterialPlan plan = PlanFrom(kind, materials);
        string needs = ModuleCrafterLocalization.T("preview_needs", ModuleCrafterLocalization.FormatRequirements(kind));
        if (!plan.HasRequired)
        {
            preview.SetText(ModuleCrafterLocalization.T("preview_missing") + needs);
            return;
        }

        if (!MachineHelper.CanPower(machine))
        {
            preview.SetText(ModuleCrafterLocalization.T("preview_no_power"));
            return;
        }

        ModuleCrafterRecipe recipe = ModuleCrafterRules.GetRecipe(kind);
        int quality = ModuleHelper.GetAjustedQuality(machine);
        int successReady = ModuleCrafterRules.ComputeSuccessPercent(kind, quality, plan.ExtraElectronics, plan.UsedIntactHousing);
        int baseSpeed = machine.GetTagReadonly("BASE_PROCESSING_SPEED_TAG").GetInt();
        int currentSpeed = machine.GetTagReadonly("CURRENT_PROCESSING_SPEED_TAG").GetInt();
        int nights = ModuleCrafterRules.ComputeActualNights(recipe.BaseNights, baseSpeed, currentSpeed);
        string cap = ModuleCrafterRules.IsSoftCapped(kind, quality, plan.ExtraElectronics, plan.UsedIntactHousing)
            ? ModuleCrafterLocalization.T("preview_capped")
            : "";
        preview.SetText(ModuleCrafterLocalization.T("preview_ready", successReady, cap, nights) + needs + RefundPreviewText(plan.RefundKind));
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

    private static ModuleCrafterMaterialPlan PlanFrom(ModuleCrafterChipKind kind, GameGridInventory materials)
    {
        List<string> ids = new List<string>();
        foreach (GameItem item in SlotItems(materials))
        {
            ids.Add(item.identifier);
        }

        return ModuleCrafterRules.PlanMaterials(kind, ids);
    }

    private static void LockPlannedMaterials(GameGridInventory materials, ModuleCrafterMaterialPlan plan)
    {
        if (materials == null)
        {
            return;
        }

        List<GameItem> items = new List<GameItem>(SlotItems(materials));
        foreach (int index in plan.ConsumedIndices)
        {
            if (index < 0 || index >= items.Count)
            {
                continue;
            }

            GameItem item = items[index];
            if (item == null)
            {
                continue;
            }

            GeneralHelper.LockItem(item);
            item.EnableTag(LockedInputTag);
        }
    }

    private static void LockRefundItems(GameGridInventory materials, ModuleCrafterMaterialPlan plan)
    {
        if (materials == null || plan.RefundLockIndices == null)
        {
            return;
        }

        List<GameItem> items = new List<GameItem>(SlotItems(materials));
        foreach (int index in plan.RefundLockIndices)
        {
            if (index < 0 || index >= items.Count || items[index] == null)
            {
                continue;
            }

            GeneralHelper.LockItem(items[index]);
        }
    }

    private static ModuleCrafterRefundKind ReadRefundKind(GameItem machine)
    {
        if (machine == null || !machine.IsTag(RefundTag))
        {
            return ModuleCrafterRefundKind.None;
        }

        string value = machine.GetTagReadonly(RefundTag).GetString();
        if (string.Equals(value, ModuleCrafterRefundKind.Core.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return ModuleCrafterRefundKind.Core;
        }

        if (string.Equals(value, ModuleCrafterRefundKind.Ticket.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return ModuleCrafterRefundKind.Ticket;
        }

        return ModuleCrafterRefundKind.None;
    }

    private static void ConsumeOneTicket(GameGridInventory materials)
    {
        if (materials == null)
        {
            return;
        }

        foreach (GameItem item in SlotItems(materials))
        {
            if (item == null || item.identifier != ModuleCrafterIds.RefundTicket)
            {
                continue;
            }

            if (GeneralHelper.IsItemLocked(item))
            {
                GeneralHelper.UnlockItem(item);
            }

            GeneralHelper.DestroyGameItems(new List<GameItem> { item });
            ModuleCrafterMod.LogMsg("Consumed one refund ticket.");
            return;
        }
    }

    private static string RefundPreviewText(ModuleCrafterRefundKind kind)
    {
        if (kind == ModuleCrafterRefundKind.Core)
        {
            return ModuleCrafterLocalization.T("preview_refund_core");
        }

        if (kind == ModuleCrafterRefundKind.Ticket)
        {
            return ModuleCrafterLocalization.T("preview_refund_ticket");
        }

        return "";
    }

    private static void ConsumeLockedMaterials(GameGridInventory materials)
    {
        List<GameItem> doomed = new List<GameItem>();
        foreach (GameItem item in SlotItems(materials))
        {
            if (item != null && item.IsTag(LockedInputTag))
            {
                doomed.Add(item);
            }
        }

        GeneralHelper.DestroyGameItems(doomed);
    }

    private static void UnlockJob(GameItem machine, GameSlotInventory chip, GameGridInventory materials)
    {
        GameItem chipItem = chip != null ? chip.childItem : null;
        if (chipItem != null)
        {
            GeneralHelper.UnlockItem(chipItem);
        }

        UnlockInventoryItems(materials);
        UnlockInventoryItems(GetHeld(machine));
        MachineHelper.UnlockModuleInv(machine);
    }

    private static void UnlockInventoryItems(GameInventory inventory)
    {
        foreach (GameItem item in SlotItems(inventory))
        {
            if (item == null)
            {
                continue;
            }

            if (item.IsTag(LockedInputTag))
            {
                item.DisableTag(LockedInputTag);
            }

            if (GeneralHelper.IsItemLocked(item))
            {
                GeneralHelper.UnlockItem(item);
            }
        }
    }


    private static GameGridInventory GetHeld(GameItem machine)
    {
        return MachineHelper.GetSlot(machine, HeldSlotIndex) as GameGridInventory;
    }

    private static bool HasOutputItem(GameSlotInventory output)
    {
        return output != null && output.childItem != null;
    }


    private static void AddCompletionNightLog(
        ModuleCrafterChipKind kind,
        bool passed,
        bool refundMaterials,
        bool skipNightCheck)
    {
        if (!ModuleCrafterRules.ShouldWriteCompletionNightLog(!skipNightCheck))
        {
            return;
        }

        try
        {
            if (PlayerStore.Instance == null)
            {
                return;
            }

            string text = ModuleCrafterLocalization.T(
                ModuleCrafterRules.CompletionNightLogKey(passed, refundMaterials),
                ModuleCrafterLocalization.KindName(kind));
            PlayerStore.Instance.AddNightLog(
                text,
                ModuleCrafterRules.CompletionNightLogColor(passed, refundMaterials));
        }
        catch (Exception ex)
        {
            ModuleCrafterMod.LogMsg("Could not write completion night log: " + ex);
        }
    }

    private static void ApplyCraftedModuleEffects(GameItem module)
    {
        if (module == null)
        {
            return;
        }

        string[] effects = ModuleCrafterRules.RollCraftedModuleEffects(NextExclusive);
        for (int i = 0; i < effects.Length; i++)
        {
            ModuleEffectHelper.AddEffectToModule(module, effects[i]);
        }

        if (effects.Length > 0)
        {
            ModuleCrafterMod.LogMsg("Crafted module effects: " + string.Join(", ", effects));
        }
    }

    // Matches vanilla loot: common modules roll square / L / T / cut-corner,
    // and overclock, eco, and fineness roll default / S / H, including the
    // official stat and value multipliers. Streamlining and neural cores have
    // no official shape variants.
    private static void ApplyVanillaRandomShape(ModuleCrafterChipKind kind, GameItem module)
    {
        if (module == null)
        {
            return;
        }

        switch (kind)
        {
            case ModuleCrafterChipKind.Performance:
            case ModuleCrafterChipKind.Efficiency:
            case ModuleCrafterChipKind.Quality:
                PreBuiltItemHelper.SetBasicModuleShape(module);
                break;
            case ModuleCrafterChipKind.Overclock:
            case ModuleCrafterChipKind.Eco:
            case ModuleCrafterChipKind.Fineness:
                PreBuiltItemHelper.SetAdvancedModuleShape(module);
                break;
        }
    }

    private static bool TryPlaceOutput(GameSlotInventory output, GameItem result)
    {
        if (result == null)
        {
            return false;
        }

        if (output == null)
        {
            return false;
        }

        return TryMoveInto(output, result);
    }

    private static bool TryMoveInto(GameInventory inventory, GameItem item)
    {
        if (inventory == null || item == null)
        {
            return false;
        }

        Func<GameItem, GameInventory, bool> previous = inventory.mayInventoryAddItemFunc;
        inventory.mayInventoryAddItemFunc = null;
        int remaining = GraphUtils.TryAcceptAll(inventory, item);
        inventory.mayInventoryAddItemFunc = previous;
        return remaining == 0;
    }

    private static void StashPlannedMaterials(GameItem machine, GameGridInventory materials, ModuleCrafterMaterialPlan plan)
    {
        GameGridInventory held = GetHeld(machine);
        if (held == null || materials == null || plan.ConsumedIndices == null)
        {
            return;
        }

        List<GameItem> items = new List<GameItem>(SlotItems(materials));
        foreach (int index in plan.ConsumedIndices)
        {
            if (index < 0 || index >= items.Count)
            {
                continue;
            }

            GameItem item = items[index];
            if (item == null)
            {
                continue;
            }

            if (!TryMoveInto(held, item))
            {
                ModuleCrafterMod.LogMsg("Could not stash a planned material; it will stay in the materials bay.");
            }
        }
    }

    private static void ReturnHeldMaterials(GameItem machine, GameGridInventory materials)
    {
        GameGridInventory held = GetHeld(machine);
        if (held == null)
        {
            return;
        }

        List<GameItem> items = new List<GameItem>(SlotItems(held));
        foreach (GameItem item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (!TryMoveInto(materials, item))
            {
                GraphUtils.EmporiumTryAdd(item);
                ModuleCrafterMod.LogMsg("A cancelled material could not return to the bay; sent to shop inventory.");
            }
        }
    }

    private static bool MayRouteOntoMachine(GameItem incoming, GameItem machine)
    {
        if (incoming == null || machine == null || incoming == machine)
        {
            return false;
        }

        if (incoming.IsTag("power_source_item"))
        {
            return true;
        }

        if (ModuleCrafterIds.TryGetChipKind(incoming.identifier, out _))
        {
            GameItem current = (MachineHelper.GetSlot(machine, ChipSlotIndex) as GameSlotInventory)?.childItem;
            if (IsWorking(machine) && current != null)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private static void RouteOntoMachine(GameItem incoming, GameItem machine)
    {
        if (!MayRouteOntoMachine(incoming, machine))
        {
            return;
        }

        GameSlotInventory slot = null;
        if (incoming.IsTag("power_source_item"))
        {
            slot = MachineHelper.GetBatterySlot(machine);
        }
        else if (ModuleCrafterIds.TryGetChipKind(incoming.identifier, out _))
        {
            slot = MachineHelper.GetSlot(machine, ChipSlotIndex) as GameSlotInventory;
        }

        if (slot == null)
        {
            return;
        }

        GameItem current = slot.childItem;
        if (current == null)
        {
            TryMoveInto(slot, incoming);
            return;
        }

        if (current == incoming)
        {
            return;
        }

        GraphUtils.SwapItemsInPlace(incoming, current);
    }

    private static ModuleCrafterChipKind ReadKind(GameItem machine, GameSlotInventory chip)
    {
        if (machine != null && machine.IsTag(KindTag))
        {
            string raw = machine.GetTagReadonly(KindTag).GetString();
            if (Enum.TryParse(raw, out ModuleCrafterChipKind parsed))
            {
                return parsed;
            }
        }

        GameItem chipItem = chip != null ? chip.childItem : null;
        if (chipItem != null && ModuleCrafterIds.TryGetChipKind(chipItem.identifier, out ModuleCrafterChipKind kind))
        {
            return kind;
        }

        return ModuleCrafterChipKind.Performance;
    }

    private static IEnumerable<GameItem> SlotItems(GameInventory inventory)
    {
        if (inventory == null || inventory.childItems == null)
        {
            yield break;
        }

        foreach (GameItem item in inventory.childItems)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static bool IsWorking(GameItem machine)
    {
        return machine != null && MachineProgressHelper.GetMachineState(machine) == MachineProgressHelper.STATE_WORKING;
    }

    private static int NextExclusive(int exclusive)
    {
        return RNG.GetRandomIntExculsive(0, exclusive);
    }

    private static void SetInt(GameItem item, string tag, int value)
    {
        item.ModifyTag(tag, delegate(TagState state)
        {
            state.Enable().SetInt(value);
        });
    }

    private static void SetString(GameItem item, string tag, string value)
    {
        item.ModifyTag(tag, delegate(TagState state)
        {
            state.Enable().SetString(value);
        });
    }

    private static void ClearTag(GameItem item, string tag)
    {
        if (item != null && item.IsTag(tag))
        {
            item.DisableTag(tag);
        }
    }
}
