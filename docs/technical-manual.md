# 2.x 技术手册

## 运行环境

- Unity 2021.3.45f2，Mono，游戏 0.46D。
- 官方 `IMod + Harmony`，目标框架 `netstandard2.1`。
- 所有游戏交互从主线程执行；动画使用 `ModHelper` 管理的协程。刷新器在读档完成后启动：`OnEnable` 阶段 `LoadedMod.IsEnabled` 尚未建立，官方延时器会返回 `null`，此时不得把刷新器记作已启动。
- 不修改游戏程序集，不补丁模组加载器或信任系统，不自行解码 PNG。

## 资源加载

游戏自动加载 `Textures/*.png` 与 `Textures/Items/*.png`，不递归读取 `Textures/Enhance/`。项目保留交接包的 `Textures/Enhance/` 原目录，同时将需要运行的透明 PNG 平铺映射到 `Textures/`。运行时只从本模组 `LoadedMod.Textures` 取贴图并创建 `Sprite`，设置点采样和 Clamp。

物品外观通过 `GameItem.getItemSpriteModifierForItemFunc` 返回 `ItemSpriteModifier`；角标、边框和动画只包含交付的透明像素。外观选择使用模组 ID、`spritePath` 形状后缀和 N。独立强化器使用 `Textures/Items/module_crafter_enhancer.png`，48×48，并在创建和读档时显式设置与制造机一致的完整 3×3 占格，避免透明中心采样缺格。

+12 双帧和结果动画的周期刷新只调用 `HandleInnateModSprite()` 与 `SpriteAnimationHelper.SyncSpriteAnimation()`。不得调用 `Validate()` 或 `ValidateSelect(false)`：前者会关闭拖拽图标，后者虽跳过背景重置，却仍会通过 `UpdateRectTransform()` 把 `fakeMiddle.rectTransform.anchoredPosition` 写回物品原位置。

`HandleInnateModSprite()` 自身会启用原位叠层并禁用拖拽叠层，因此刷新前须记录 `background.image.enabled`、`fakeBackground.image.enabled` 和拖拽颜色；刷新后通过 `GetActiveInnateSpriteMods()` 恢复原位叠层的 enabled 状态。只有拖拽图标原本已显示时，才以原状态调用 `ToggleFakeBackground(true, originalColor)` 恢复其叠层，不得强制调用 `ToggleFakeBackground(false)` 或改写拖拽位置。

音效放在官方 `Sounds/` 根目录，使用 44100 Hz、单声道、16 位 PCM WAV，通过 `ModHelper.PlaySound(module_crafter, key)` 播放。`ModuleCrafterEnhanceRules.ResultSoundKey` 将成功映射到打铁声，将维持、掉级、稳固夹归零映射到普通失败声，将无稳固夹报废映射到独立破碎声。事务应用结果后才播放；资源缺失或播放异常只写模组日志，不影响强化结果。

## 机器与存档适配

`ModuleCrafterEnhancerMachine` 注册独立强化器，以 `MachineryHelper.InitMachinery(machine, 0)` 初始化零功耗机器，不创建电池、模组舱、芯片、输出或夜间进度槽。一个 8×6 `GameGridInventory` 通过 `mayInventoryAddItemFunc` 限制为一个可强化模组及触媒、稳固夹、废金属。

`ModuleCrafterMachine` 保留制造槽位 0–5 和原制造流程。旧强化槽索引 6 不再加入窗口；读档时解锁其中的物品并通过 `GraphUtils.EmporiumTryAdd` 退回商店库存。

`ModuleCrafterEnhanceState` 负责标签读写，`ModuleCrafterEnhanceRules` 保持无 Unity 依赖。任何已经强化过的物品即使 N=0 也保留等级和 B 标签，以便禁用后恢复。预览使用 `GetBaseStatsForPreview` 的只读路径，不因打开面板迁移旧物品。

初始化顺序：识别九种完好模组 → 若无 B 则建立快照 → 若存在学习算法且 B 缺失，以 `LEARNING_ALGO_MAX_* / 2` 恢复正值 B → 按 N 重算负向维、正向学习上限和训练进度 → 更新临时三围与售价 → 注册原生叠加层。

禁用时不删除存档标签，只把三个永久三围恢复 B，重设临时三围和未强化售价，并停止动画、注销事件、清理 Harmony。

## 事务顺序

预览不消费任何对象。确认时取得本机材料栏内同一物品引用并重新校验，随后支付金币和栏内触媒，再掷骰。2.1.8 起，在资格检查通过之后、扣费之前调用 `StartingPerk.IsTinkerer()`；拥有该特性时以 20% 概率免除本次金币和材料消耗，无特性时不消耗随机数。损毁需要把原材料栏中的目标替换为 `system_module_ruined`，清除强化和特殊词条；普通付费报废返还 1 触媒，免费报废因未消耗触媒而不得返还。产出优先放回原栏，失败则退到商店库存。稳固夹结果只消耗 1 夹、N=0，保留原物品和词条；免费命中时保留该夹但仍归零。

兑换先在本机栏内检查至少 3 个废金属，再创建触媒并用 `GraphUtils.CanAccept` 验证空间；失败销毁未使用的临时触媒且不扣废金属。通过检查后才消耗废金属并放入触媒，若重校验后仍发生溢出，则退到商店库存并记录日志，不静默丢失。

## 商店与兼容点

- 杰克逊强化器货骰追加在原货表掷骰之后；`ModuleCrafterJacksonStockKey.Enhancer = 10`，2.1.2 追加 `Catalyst = 11`、`SafeguardClip = 12`，位掩码仍兼容已有整数存储。拥有状态按物品 ID 检测。
- 杰克逊普通／进阶芯片、触媒、回流券概率为 50%，稳固夹、回流核心为 20%；进阶芯片和回流核心保留第 21 天门槛。触媒和稳固夹货骰位于旧货表之后，避免改变旧物品随机序列。
- 退休商人原货表结构新增 `EnhancerCount = 1`，强化材料继续使用按天独立缓存，不影响旧怜悯状态。2.1.8 新增 `RuinedHousingCount`，固定 1 个并以 50% 追加 1 个；其货骰追加在全部旧货骰之后。
- 制造软顶通过带最高 N 的重载计算，旧重载保留。
- 蒸发器只对 `Evaporator()` 创建委托中特定的质量 Clamp 做窄 IL 修改。
- `ModuleEffectHelper.Degrade` 仅对带本模组等级标签的物品走 B 劣化，其它物品执行原版。
- 其它客人通过 `OnGenerateCustomerLate` 包装其最终对话动作追加物品；废料场通过成功掉落路径的窄补丁追加，且不进入扫描仪第二抽。2.1.8 在同一 Postfix 中先保留原 8% 触媒货骰，再独立掷 20% 壳体，命中后以 50%／50% 选择损坏或报废。

## 验证边界

纯规则测试证明公式、表和分支；编译证明引用与方法签名；官方扫描证明扫描器结果。三者都不替代游戏内的 UI、存档迁移、禁用恢复、材料隔离、商店上架、来源概率和制造／蒸发器联动验收。
