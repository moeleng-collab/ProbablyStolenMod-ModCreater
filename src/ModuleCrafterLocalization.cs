#nullable disable
using System;

public static class ModuleCrafterLocalization
{
    public static string T(string key, params object[] args)
    {
        string locale = "en";
        try
        {
            locale = LocHelper.GetCurrentLocaleCode() ?? "en";
        }
        catch
        {
            locale = "en";
        }

        bool zh = locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        string text = zh ? Zh(key) : En(key);
        if (string.IsNullOrEmpty(text))
        {
            text = En(key);
        }

        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException)
            {
                return text;
            }
        }

        return text;
    }

    private static string En(string key)
    {
        return key switch
        {
            "machine_name" => "Module Crafter",
            "machine_desc" => "Crafts universal modules from a recipe chip and materials overnight.",
            "machine_flavor" => "A boxy fabricator with a chip bay on top and a module clamp inside.",
            "chip_name" => "{0} Recipe Chip",
            "chip_desc" => "Reusable. Insert into the Module Crafter to start a job.",
            "mat_housing" => "Damaged or intact housing ×{0}",
            "mat_item" => "{0} ×{1}",
            "mat_join" => ", ",
            "preview_needs" => " Needs: {0}.",
            "label_module" => "Modules",
            "label_chip" => "Chip",
            "label_materials" => "Materials",
            "label_output" => "Output",
            "label_control" => "Control",
            "btn_start" => "Start",
            "btn_cancel" => "Cancel",
            "preview_empty" => "Insert a recipe chip.",
            "preview_missing" => "Missing required materials.",
            "preview_no_power" => "No power.",
            "preview_output_full" => "Crafting is finished. Collect the item from the output slot.",
            "preview_ready" => "Success {0}%{1}. {2} night(s).",
            "preview_capped" => " (soft cap)",
            "preview_working" => "Working {0}/{1}. Success locked at {2}%.",
            "preview_paused_output" => "Output occupied. Progress paused.",
            "preview_paused_power" => "Not enough power. Progress paused.",
            "preview_result_success" => "Success (rolled {0}, needed <{1}%).",
            "preview_result_fail" => "Failed (rolled {0}, needed <{1}%).",
            "preview_result_fail_refund" => "Failed (rolled {0}, needed <{1}%). Materials returned.",
            "preview_refund_ticket" => " Fail refund ready; the ticket is consumed when you start.",
            "preview_refund_core" => " Fail refund ready; the core will stay.",
            "ticket_name" => "Reflux Ticket",
            "ticket_desc" => "On failure, recipe materials are returned instead of a broken module. Consumed when you start a job; cancel does not return it.",
            "ticket_flavor" => "A one-job insurance slip for the module crafter.",
            "core_name" => "Reflux Core",
            "core_desc" => "On failure, recipe materials are returned instead of a broken module. Never consumed.",
            "core_flavor" => "A permanent safeguard seated in the materials bay.",
            "tooltip_ready" => "Ready",
            "tooltip_working" => "Working {0}/{1}",
            "tooltip_finished" => "Finished. Collect the output.",
            "nightlog_success" => "The Module Crafter finished a {0} module. Collect it from the output slot.",
            "nightlog_fail" => "The Module Crafter failed a {0} job. Collect the item from the output slot.",
            "nightlog_fail_refund" => "The Module Crafter failed a {0} job. Recipe materials were returned.",
            "kind_performance" => "Performance",
            "kind_efficiancy" => "Efficiency",
            "kind_quality" => "Quality",
            "kind_overclock" => "Overclock",
            "kind_eco" => "Eco",
            "kind_fineness" => "Refine",
            "kind_streamlining" => "Optimize",
            "kind_neural_capped" => "Capped Neural Core",
            "kind_neural_uncapped" => "Uncapped Neural Core",
            "merchant_name" => "Retired Module Merchant",
            _ => key
        };
    }

    private static string Zh(string key)
    {
        return key switch
        {
            "machine_name" => "模组制造机",
            "machine_desc" => "用配方芯片和材料，经夜间加工制造通用模组。",
            "machine_flavor" => "箱形制造机，顶部是芯片槽，里面是喷头和模组夹具。",
            "chip_name" => "{0}配方芯片",
            "chip_desc" => "可重复使用。插入模组制造机即可开工。",
            "mat_housing" => "损坏或完好壳体 ×{0}",
            "mat_item" => "{0} ×{1}",
            "mat_join" => "、",
            "preview_needs" => " 需要：{0}。",
            "label_module" => "模组舱",
            "label_chip" => "芯片",
            "label_materials" => "材料",
            "label_output" => "输出",
            "label_control" => "控制",
            "btn_start" => "开造",
            "btn_cancel" => "取消",
            "preview_empty" => "请插入配方芯片。",
            "preview_missing" => "必需材料不足。",
            "preview_no_power" => "未通电。",
            "preview_output_full" => "制造已完成，请从输出槽取走物品。",
            "preview_ready" => "成功率 {0}%{1}，预计 {2} 晚。",
            "preview_capped" => "（已触顶）",
            "preview_working" => "制造中 {0}/{1}，成功率已锁定为 {2}%。",
            "preview_paused_output" => "输出槽被占用，进度暂停。",
            "preview_paused_power" => "电力不足，进度暂停。",
            "preview_result_success" => "制造成功（掷出 {0}，需小于 {1}%）。",
            "preview_result_fail" => "制造失败（掷出 {0}，需小于 {1}%）。",
            "preview_result_fail_refund" => "制造失败，材料已退回（掷出 {0}，需小于 {1}%）。",
            "preview_refund_ticket" => " 失败将退回材料，回流券在开造时消耗。",
            "preview_refund_core" => " 失败将退回材料，回流核心保留。",
            "ticket_name" => "回流券",
            "ticket_desc" => "失败时退回全部配方材料，不产出损坏或报废。点击开造时立即消耗，取消不退回。",
            "ticket_flavor" => "一张只保一次制造的回流单据。",
            "core_name" => "回流核心",
            "core_desc" => "失败时退回全部配方材料，不产出损坏或报废。可永久使用。",
            "core_flavor" => "嵌在材料栏里的永久回流装置。",
            "tooltip_ready" => "就绪",
            "tooltip_working" => "制造中 {0}/{1}",
            "tooltip_finished" => "已完成，请取走输出。",
            "nightlog_success" => "模组制造机已完成{0}的制造，请从输出槽取走模组。",
            "nightlog_fail" => "模组制造机的{0}制造失败，请从输出槽取走物品。",
            "nightlog_fail_refund" => "模组制造机的{0}制造失败，材料已退回。",
            "kind_performance" => "性能",
            "kind_efficiancy" => "效率",
            "kind_quality" => "质量",
            "kind_overclock" => "超频",
            "kind_eco" => "环保",
            "kind_fineness" => "精炼",
            "kind_streamlining" => "优化",
            "kind_neural_capped" => "神经核心（受限）",
            "kind_neural_uncapped" => "神经核心（无限制）",
            "merchant_name" => "退休模组商人",
            _ => ""
        };
    }


    public static string FormatRequirements(ModuleCrafterChipKind kind)
    {
        ModuleCrafterRecipe recipe = ModuleCrafterRules.GetRecipe(kind);
        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
        foreach (ModuleCrafterMaterialNeed need in recipe.Required)
        {
            if (need.AllowCorruptOrRuinedHousing)
            {
                parts.Add(T("mat_housing", need.Count));
            }
            else
            {
                parts.Add(T("mat_item", ItemDisplayName(need.ItemId), need.Count));
            }
        }

        return string.Join(T("mat_join"), parts);
    }

    public static string ItemDisplayName(string itemId)
    {
        string name = LocHelper.GetLocalizedItem("item_" + itemId + "_name");
        return string.IsNullOrEmpty(name) ? itemId : name;
    }

    public static string KindName(ModuleCrafterChipKind kind)
    {
        return kind switch
        {
            ModuleCrafterChipKind.Performance => T("kind_performance"),
            ModuleCrafterChipKind.Efficiency => T("kind_efficiancy"),
            ModuleCrafterChipKind.Quality => T("kind_quality"),
            ModuleCrafterChipKind.Overclock => T("kind_overclock"),
            ModuleCrafterChipKind.Eco => T("kind_eco"),
            ModuleCrafterChipKind.Fineness => T("kind_fineness"),
            ModuleCrafterChipKind.Streamlining => T("kind_streamlining"),
            ModuleCrafterChipKind.NeuralCapped => T("kind_neural_capped"),
            ModuleCrafterChipKind.NeuralUncapped => T("kind_neural_uncapped"),
            _ => kind.ToString()
        };
    }
}
