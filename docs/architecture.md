# 2.0 模块划分

- `ModuleCrafterRules.cs`：1.0 制造规则及带强化软顶溢出的兼容重载。
- `ModuleCrafterEnhanceRules.cs`：无 Unity 依赖的强化公式、费用、失败、价格、来源和动画帧规则。
- `ModuleCrafterEnhanceState.cs`：游戏物品 B/N 标签、学习算法、售价、读档/禁用恢复和劣化补丁。
- `ModuleCrafterEnhancerMachine.cs`：独立强化器的物品注册、3×3 外观、共享材料栏、强化／兑换按钮及读档。
- `ModuleCrafterEnhancement.cs`：材料栏目标查找、只读预览、确认事务、消耗、报废替换与兑换；不读取玩家全局材料库存。
- `ModuleCrafterEnhanceVisuals.cs`：官方贴图获取、形状映射、原生叠加层及可打断动画；只刷新视觉并保留原位／拖拽显示状态，不修改拖拽坐标。
- `ModuleCrafterEnhanceAudio.cs`：通过官方 `ModHelper.PlaySound` 播放三类结果音效，播放失败只记录日志。
- `ModuleCrafterEnhanceSources.cs`：退休商人按天货表、客人和废料场附加来源。
- `ModuleCrafterMachine.cs`：保留原制造槽位和流程与有效软顶，仅保留隐藏旧强化槽索引用于读档迁移。
- `ModuleCrafterStock.cs`：杰克逊强化器货骰及拥有状态检测。
- `ModuleCrafterMod.cs`：注册/注销生命周期入口。

依赖方向为 UI/游戏适配层 → 存档适配层 → 纯规则；纯规则不引用游戏类型。新物品、外观和来源不进入一期制造材料规划。
