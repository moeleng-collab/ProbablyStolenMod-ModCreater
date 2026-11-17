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
            "mat_housing" => "Damaged or ruined housing ×{0}",
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
            "enhancer_name" => "Module Enhancer",
            "enhancer_desc" => "Enhances modules instantly using only its own material bay. No battery or nightly processing.",
            "enhancer_flavor" => "A London-pattern anvil bolted to an iron-banded wooden stump.",
            "enhancer_tooltip" => "Instant daytime enhancement. Uses only this machine's material bay; no power required.",
            "catalyst_name" => "Enhancement Catalyst",
            "catalyst_desc" => "Consumed during module enhancement. Stackable.",
            "catalyst_flavor" => "A measured dose of very expensive potential.",
            "clip_name" => "Stabilizing Clip",
            "clip_desc" => "On a failed +11 to +12 attempt, consumed to prevent destruction and reset the module to +0. Does not prevent level drops.",
            "clip_flavor" => "It holds the module together when the numbers do not.",
            "tooltip_enhance_level" => "Enhancement: +{0}",
            "tooltip_enhance_next" => "Next level: {0}% success",
            "enhance_empty" => "Place one intact craftable module in the material bay.",
            "enhance_invalid" => "This item cannot be enhanced.",
            "enhance_max" => "This module has reached +12.",
            "enhance_ready" => "Enhance +{0} to +{1}: {2}% success. Cost: {3} gold and {4} catalyst(s). Clips in bay: {5}.",
            "enhance_stats" => "\nP/E/Q: {0}/{1}/{2} → {3}/{4}/{5}.",
            "enhance_break_both" => "\nCurrent and next levels exceed this module's native cap.",
            "enhance_break_next" => "\nThe next level will exceed this module's native cap.",
            "enhance_failure_guaranteed" => "\nFailure: none (guaranteed success).",
            "enhance_failure_stay" => "\nFailure: level stays unchanged.",
            "enhance_failure_drop1" => "\nFailure: level drops by 1.",
            "enhance_failure_drop3" => "\nFailure: level drops by 3.",
            "enhance_failure_ruin" => "\nFailure without a clip: ruined; this attempt's catalyst is refunded.",
            "enhance_failure_clip" => "\nFailure: consumes 1 clip and resets the module to +0.",
            "enhance_need_gold" => "\nNot enough gold ({0} required).",
            "enhance_need_catalyst" => "\nNot enough catalysts in this material bay ({0} required).",
            "enhance_result_success" => "Enhancement succeeded: +{0} to +{1}.",
            "enhance_result_stay" => "Enhancement failed. The module stayed at +{0}.",
            "enhance_result_drop" => "Enhancement failed. The module dropped from +{0} to +{1}.",
            "enhance_result_ruined" => "Enhancement failed. The module was ruined.",
            "enhance_result_clip" => "Enhancement failed. The stabilizing clip was consumed and the module reset to +0.",
            "exchange_missing" => "Exchange requires 3 scrap metal in this material bay.",
            "exchange_done" => "Exchanged 3 scrap metal for 1 enhancement catalyst.",
            "exchange_no_space" => "No space for the exchanged catalyst. Nothing was consumed.",
            "exchange_hint" => "\nExchange: 3 scrap metal → 1 catalyst (bay: {0}/3).",
            "btn_enhance" => "Enhance",
            "btn_exchange" => "Exchange catalyst",
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
            "mat_housing" => "损坏或报废壳体 ×{0}",
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
            "enhancer_name" => "模组强化器",
            "enhancer_desc" => "只使用本机材料栏即时强化模组，不需要电池，也不进行夜间加工。",
            "enhancer_flavor" => "固定在铁箍木墩上的伦敦式铁砧。",
            "enhancer_tooltip" => "白天即时强化，只读取本机材料栏，无需供电。",
            "catalyst_name" => "强化触媒",
            "catalyst_desc" => "强化模组时消耗，可堆叠。",
            "catalyst_flavor" => "一小份精确计量的昂贵可能性。",
            "clip_name" => "稳固夹",
            "clip_desc" => "+11→+12 失败时消耗，防止损毁并将模组归零；不防掉级。",
            "clip_flavor" => "当数字靠不住时，至少它还能夹住模组。",
            "tooltip_enhance_level" => "强化等级：+{0}",
            "tooltip_enhance_next" => "下一级成功率：{0}%",
            "enhance_empty" => "请在材料栏放入一个可制造的完好模组。",
            "enhance_invalid" => "该物品无法强化。",
            "enhance_max" => "该模组已达到 +12。",
            "enhance_ready" => "强化 +{0} → +{1}：成功率 {2}%，消耗 {3} 金币和 {4} 个触媒。材料栏稳固夹：{5}。",
            "enhance_stats" => "\n性能／效率／质量：{0}／{1}／{2} → {3}／{4}／{5}。",
            "enhance_break_both" => "\n当前与下一级均已突破该模组固有上限。",
            "enhance_break_next" => "\n下一级将突破该模组固有上限。",
            "enhance_failure_guaranteed" => "\n失败结果：无（必定成功）。",
            "enhance_failure_stay" => "\n失败结果：等级维持。",
            "enhance_failure_drop1" => "\n失败结果：降低 1 级。",
            "enhance_failure_drop3" => "\n失败结果：降低 3 级。",
            "enhance_failure_ruin" => "\n无稳固夹时失败：模组报废，返还本次触媒。",
            "enhance_failure_clip" => "\n失败结果：消耗 1 个稳固夹，模组归零。",
            "enhance_need_gold" => "\n金币不足，需要 {0}。",
            "enhance_need_catalyst" => "\n本机材料栏触媒不足，需要 {0} 个。",
            "enhance_result_success" => "强化成功：+{0} → +{1}。",
            "enhance_result_stay" => "强化失败，模组保持 +{0}。",
            "enhance_result_drop" => "强化失败，模组从 +{0} 降至 +{1}。",
            "enhance_result_ruined" => "强化失败，模组已损毁。",
            "enhance_result_clip" => "强化失败，稳固夹已消耗，模组重置为 +0。",
            "exchange_missing" => "兑换需要本机材料栏内有 3 个废金属。",
            "exchange_done" => "已用 3 个废金属兑换 1 个强化触媒。",
            "exchange_no_space" => "材料栏没有空间存放兑换的触媒，未消耗任何物品。",
            "exchange_hint" => "\n兑换：3 个废金属 → 1 个触媒（材料栏 {0}/3）。",
            "btn_enhance" => "强化",
            "btn_exchange" => "兑换触媒",
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
