# ModuleCrafter 开发者模式

> 用于快速验收 3.0 功能。请只在测试存档中使用，不要把调试物品、修改后的日期、升级状态或迁移结果当作正式进度。

## 启用方式

开发者模式已与正式构建分离：

- **正式发布／默认构建**：在游戏根目录执行 `python Modding/ModuleCrafter/build.py`。不会编译 `ModuleCrafterDebug.cs`，不会注册 F9 命令，不会复制 `debug.on`，清单只声明 `reflection-external`。
- **开发构建**：执行 `python Modding/ModuleCrafter/build.py --dev`。该配置定义 `MC_DEBUG`，编译调试命令、复制 `debug.on`，并仅为开发清单追加 `files` 能力，用于检测标记文件。
- 从开发构建切回默认构建时，安装目录中的 `debug.on` 会自动删除，因此 F9 调试入口和 `files` 权限会一起消失。

启用开发构建后，在游戏中启用 ModuleCrafter 并重启，再按 **F9**，在 **Modding Debug Panel** 中使用 `MC` 和 `MC3` 命令。请只上传默认构建，不要上传开发构建。

## 推荐快速验收流程

### 1. 专精、制造与组件

1. `MC3: Grant full kit`：取得制造机、全部 28 种芯片和模组、组件、主题材料与原生测试机器。
2. `MC3: Recipe universal`：切换到通用配方；`MC3: Components x20` 补足组件。
3. 放入芯片和材料，使用 `MC: Force next success` 或 `MC: Force next failure`，再使用 `MC: Finish current job`。
4. 重复 `MC3: Recipe themed` 验证主题配方。
5. 检查大组组件拆分、取消退料、失败消耗和回流返还。

### 2. 强化上限与专用效果

1. `MC3: Relock precision`：专用模组恢复 +8 上限。
2. `MC3: Modules +8`，尝试继续强化，应提示需要「精密强化工艺」。
3. `MC3: Unlock all upgrades`，再尝试 +9→+10、+10→+11、+11→+12。
4. 在 +10、+11、+12 分别检查实际机器效果；+11 必须与 +10 相同。
5. `MC3: Modules +0` 可重置全部测试模组。

### 3. 王尔德与货源

1. `MC3: Unlock all upgrades`：一次解锁五项王尔德升级，同时验证即时奖励。
2. `MC3: Stock all channels`：追加合法、退休和黑市货源。
3. `MC: Queue retired merchant`：验证退休模组商人的合法芯片与组件。
4. `MC: Stock thief`／`MC: Stock smuggler`：验证违禁芯片渠道和 150% 订货价。
5. `MC3: Dump state`：检查升级状态、模组 N、V0 和当前售价。

### 4. 旧存档迁移

1. 备份测试存档。
2. `MC3: Run migration`：重置迁移版本并重新执行。
3. 连续执行两次，确认没有重复发物品、重复涨价或重复改变货表。
4. `MC3: Dump state`：核对 Legacy 任务、V0、违禁等级和升级状态。

## 3.0 命令清单

| 命令 | 作用 |
| --- | --- |
| `MC3: Grant full kit` | 全部 28 种芯片与模组、制造机、强化器、组件、主题材料和原生测试机器 |
| `MC3: Components x20` | 追加 20 个通用制造组件 |
| `MC3: Grant native machines` | 追加水培、DNA、打印、熔炉、蒸发器、警报和化学设备 |
| `MC3: Modules +0/+8/+10/+12` | 将全部可强化模组设为指定等级并重算状态 |
| `MC3: Unlock all upgrades` | 解锁五项王尔德升级并触发购买奖励 |
| `MC3: Relock precision` | 重新锁定精密强化工艺，专用模组恢复 +8 上限 |
| `MC3: Recipe themed/universal` | 切换所有制造机的配方版本 |
| `MC3: Run migration` | 将迁移版本重置后重新执行 |
| `MC3: Stock all channels` | 追加合法专用芯片、组件和黑市订货 |
| `MC3: Add 10000 cash` | 追加 10000 金币用于强化费用测试 |
| `MC3: Dump state` | 输出专精、升级、迁移版本及模组 N／V0／售价 |

原有 `MC` 命令仍可用于强制成功／失败、完成当前任务、跳过夜晚、充放电、日期调整、旧商店和退休商人。

## 验证边界

调试命令只缩短准备时间，不会自动证明功能正确。仍需逐项目测：

- 配方材料与组件数量是否守恒。
- 原生机器的真实产出、声望、纯度、工作量和特殊状态。
- 购买升级与读档后是否重复发奖励。
- 禁用／重启用后的恢复表现。
- 正式存档是否未受调试操作影响。
