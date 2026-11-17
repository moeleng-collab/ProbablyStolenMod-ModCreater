# ModuleCrafter validation (current)

Date: 2026-11-17 Asia/Shanghai

## 2.1.6 crafter self-target recursion hotfix

- User reported intermittent native crashes while inserting a module chip or electronic components, then isolated a 100% reproducible crash when moving the module crafter. The crash therefore involved the crafter's own item-targeting path rather than only a particular ingredient.
- `ModuleCrafterMachine.cs` now short-circuits both `mayItemTargetItemFunc` and `canItemTargetItemFunc` when `incoming != machine && target == machine`. This prevents the machine from recursively evaluating itself as a target of its own incoming item while preserving the existing manufacturing, enhancement and shop rules.
- Rules tests passed with `All rules assertions passed`. Official scan: `Refused: False`, `MethodsScanned: 405`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Build and installed DLLs are byte-identical, SHA256 `14afea4e1a78f0f5a4e832c853e214ae039d31ecde555c276d94e6699e4db769`. Source, installed and release manifests are all version 2.1.6.
- Initial in-game acceptance: after installing the fix, the user reported `现在暂时没有崩溃了`. This is preliminary evidence, not a complete regression of every chip, material, machine-move and save/load path.
- Version 2.1.6 was published to the existing public Steam Workshop item `3840773884`; no new Workshop item was created. The release directory contained 460 files and 437,923 bytes. Post-upload remote metadata reported `file_size=437923`, `time_updated=1794923466`, `hcontent_file=8287093524558073467`, `visibility=0` and `banned=0`; the Workshop description and changelog include 2.1.6 and 2.1.5.

## 2.1.5 seller-supply ownership hotfix

- User reported that a scavenger's enhancement catalyst could not be bought and remained after the seller left. The supplied runtime log confirms `Enhancement catalyst added by seller scavenger.` and contains no purchase exception for that interaction.
- Current-game reference inspection shows that direct-sale items need no separate client list: `DismissCurrentClient()` removes every non-owned item from both counters. The shared catalyst/clip stack predicate compared only item IDs, while grid placement prioritizes stacking. A seller's unowned supply could therefore merge into an existing player-owned stack, making the merged stack non-purchasable and exempt from departure cleanup.
- `CanStackSupplies()` now requires matching non-empty IDs and matching ownership. The catalyst and stabilizing-clip factories both use this rule; seller dialogues, probabilities, prices and other stock sources are unchanged.
- Rules tests passed with `All rules assertions passed`, including same-owner stacks allowed, mixed-owner stacks rejected in both directions, different IDs rejected and null IDs rejected. Build and local installation passed; installed manifest version is 2.1.5.
- Official scan: `Refused: False`, `MethodsScanned: 405`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Installed and build DLLs are byte-identical, SHA256 `0313bae247309f57e7c0f6a347bce15d6f3a796ed21b366e0ff53bbd18fa0dfe`. Installed and source manifests are byte-identical, SHA256 `6ba6a9996e7eab97f2cc2c3df20578e7d4cab401addf7a5f58af58ec8e839582`.
- In-game acceptance is still required after a full restart: with an owned catalyst stack on the counter, the scavenger's separate catalyst must be purchasable, become player-owned after purchase, and be removed with the seller if left unbought. Existing player stack quantity, original scavenger items, other sellers and retired-merchant stock require regression checks. Build and scan results are not transaction-lifecycle verification.

## 2.1.4 visual lifecycle and scheduler hotfix

- The supplied `Player.log` records `NullReferenceException` in `UnityEngine.Behaviour.get_enabled`, called by `ModuleCrafterEnhanceVisuals.RefreshModifiers()` during scene-resource cleanup. The old path read `element.background.image.enabled` and `element.fakeBackground.image.enabled` without checking whether those UI objects had already been destroyed.
- `RefreshModifiers()` now isolates visual exceptions and delegates to a guarded core refresh. It skips incomplete real/drag backgrounds, containers and images, checks the active-modifier collection and each modifier image, and preserves the accepted 2.1.3 refresh order, visibility restoration and fake-overlay color. No drag coordinates are written and no periodic `Validate()`／`ValidateSelect()` call was restored.
- `Tick()` now reschedules in `finally`; one visual exception can no longer leave `_running=true` with no future tick. Identical exception type/stack signatures are logged only once, and the suppression state resets on shutdown. This evidence does not establish any connection to the earlier native crash.
- Rules tests passed with `All rules assertions passed`. The normal build and local installation passed with no running game process. Official scan: `Refused: False`, `MethodsScanned: 404`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Installed DLL and build DLL are byte-identical, 116224 bytes, SHA256 `768e6bb8d3636649cec2505b7197447fd30c15d0516ec354295ed26eb3dd8ea1`. Installed and source manifests are byte-identical, SHA256 `f9a40a641fb285f83b6aac219a3779922082820834518f112b58113ec998dd1d`. Installed file version is 2.1.4.0 and informational version is 2.1.4.
- In-game acceptance is still required after a full restart: repeated scene transitions, +12 dragging across blink frames, and every success/failure result animation. Build and scan results are not gameplay verification.

## 2.1.3 +12 drag-position hotfix

- User reported that +12 dragging was broken again in installed 2.1.2. The runtime log confirms 2.1.2 was active and all three sounds loaded; no related exception was recorded.
- Current-game reference inspection found the remaining drag reset: `ValidateSelect(false)` skips `ToggleFakeBackground(false)` but still calls `UpdateRectTransform()`, whose final line writes `fakeMiddle.rectTransform.anchoredPosition = middle.rectTransform.anchoredPosition`. The 50 ms animation scheduler therefore kept snapping the dragged visual back to its inventory position.
- The refresh path now calls only `HandleInnateModSprite()` and `SpriteAnimationHelper.SyncSpriteAnimation()`, with no `Validate()`／`ValidateSelect()` layout call. It records real/fake visibility and the fake color, restores real-overlay enabled states through `GetActiveInnateSpriteMods()`, and restores fake overlays only when the dragged visual was already visible. It does not write drag coordinates.
- Rules tests passed and the compile-only 2.1.3 build succeeded. Official scan: `Refused: False`, `MethodsScanned: 402`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Build and local installation passed after the user confirmed a full game exit. The installed manifest is 2.1.3. Installed DLL SHA256: `2e7da98826a9dfa4d1a9e52f80a4c2c344a8b25d9cdadba2b9cd9853c493d44f`; manifest SHA256: `8288ca7ec6f180f1bcc1b6437a0ecb818eea86e296dc67db962eb1dac12ebcdc`. Installed DLL, manifest and all three WAV files are byte-identical to the verified build/source copies.
- In-game acceptance passed at 13:22: the user confirmed that +12 dragging and the result animations were working normally. The subsequent crash investigation did not change this build or revert its drag fix.

## 13:22 native crash investigation

- The 13:22 `Player.log` contains successful ModuleCrafter 2.1.3 initialization and all three audio loads, but no ModuleCrafter exception stack. The same native crash also occurred at 12:46.
- Both Windows application-error records report exception `0xc0000005` in `ntdll.dll` 10.0.26100.9711 at offset `0x4a612`. Both dump exception parameters show an attempted read of `0xffffffffffffffff`.
- Disassembly identifies the faulting instruction as `movaps xmmword ptr [rsp + 0x220], xmm6` in `RtlDosApplyFileIsolationRedirection_Ustr+0x192`. In both dumps `RSP mod 16 == 8`, so the aligned SIMD write targets a misaligned stack address. The direct crash mechanism is therefore a native stack-alignment failure, not an ordinary C# null reference.
- The raw native stack contains `mono-2.0-bdwgc.dll`, `UnityPlayer.dll` and `gameoverlayrenderer64.dll`, but available symbols do not establish which mod, native call or injected component triggered the misalignment. No specific responsible mod or Steam Overlay has been proven.
- The log also contains 216 repetitions of `[Next Opening: Suppliers & Odin]: Could not refresh item highlights. Object reference not set to an instance of an object`. This comes from `NextOpeningVisitors/src/DemandHighlightUI.cs` (`DemandHighlightHost.Update()`), which logs only `exception.Message`. The same error exists in older logs from 2026-11-02 and 2026-11-05, so it is a separate long-standing defect and is not by itself evidence of causing this crash.
- ModuleCrafter 2.1.3 no longer calls `GameItemElement.Validate()`／`ValidateSelect()` from its periodic refresh; Next Opening patches `GameItemElement.Validate`. Do not revert the accepted 2.1.3 drag fix during this investigation.
- Evidence and both dumps were copied to `.kimi-tmp/crash-20261117-1322/`. Recommended single-variable A/B order: first temporarily disable Next Opening's three item-highlight options and repeat the same save/+12-drag actions; if the identical crash remains, test Steam Overlay disabled. Do not disable or modify another mod without the user's approval.

## 2.1.2 stock, animation-refresh and enhancement-audio update

- Jackson now independently stocks common/advanced chips, catalysts and refund tickets at 50%, and reflux cores/stabilizing clips at 20%. Existing machine chances, day gates, the common-chip guarantee and the old random-roll sequence are preserved; neural chips are not added.
- The animation refresh path now uses `GameItemElement.ValidateSelect(updateBackground: false)`. Current-game reference inspection confirms that this retains `HandleInnateModSprite()` and `SpriteAnimationHelper.SyncSpriteAnimation(this)` while skipping the `ToggleFakeBackground(false)` drag reset caused by full validation.
- Added original PCM effects: a 0.62-second hammer strike for success, a 0.36-second dull metallic clunk for stay/drop/clip reset, and a 1.05-second heavy strike with metallic fragments for unprotected +11→+12 ruin. All three are 44100 Hz, mono, 16-bit WAV files loaded from the official `Sounds/` directory through `ModHelper.PlaySound`; playback failures are logged without changing the transaction.
- Rules tests passed, including every result-to-sound-key mapping. Compile-only build passed while the game was running; after a later process check showed no running game process, the normal build and local installation also passed. WAV format, duration and non-empty file checks passed.
- Official scan of the installed build: `Refused: False`, `MethodsScanned: 402`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Installed DLL SHA256: `20ee093277b2292e2d9856ef010ef5fd0bb6348c346382a9d70687d493029522`; installed manifest SHA256: `6715f2585352c97abe8eaf98ab102e5b3c9882ac94f63ac22967581931b41a90`. Sound SHA256 values: success `fc492097ba4b903e50c54de3bfd92adfe212c4b7a9513b1a4316993c93f5f210`, failure `c61bff324d4144ede02efaadefb0e0f0456af8f71b29e65795324dda7bf16981`, ruined `0376b7114dab4dd7bfa61525688f47eeafd4ddadc20b00496be7428dc3638b6d`.
- The installed manifest is 2.1.2. Installed DLL, manifest and all three WAV files are byte-identical to the verified build/source copies. Actual initialization, animation playback, +12 dragging, 3×3 occupancy, all three sounds and Jackson probabilities still require in-game acceptance after restart.

## 2.1.1 drag and machine-footprint hotfix

- User reported that a module could not be dragged after reaching +12, and requested the same machine footprint for the enhancer and crafter.
- Confirmed in the current game reference that `GameItemElement.Validate()` calls `ToggleFakeBackground(false, ...)`. The old +12 scheduler invoked it every 50 ms, repeatedly resetting the drag visual. All animation scheduler/play/expiry refresh paths now call the native `HandleInnateModSprite()` overlay-only path for `GameItemElement`; full validation remains only as a non-UI fallback.
- Both machine PNGs are 48×48, but centre-cell sampling produced nine opaque cells for the crafter and only seven for the anvil. Enhancer creation and load now explicitly apply `111111111` on a three-column grid, matching the crafter's full 3×3 footprint and migrating existing items.
- Rules tests passed, including full-machine-shape assertions. Build and local installation passed; installed manifest is 2.1.1.
- Official scan: `Refused: False`, `MethodsScanned: 400`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Installed DLL SHA256: `7578cc25bf0fd6a0ea372455fba8ed94df67866bac71b29c6bc699be8220a335`; manifest SHA256: `2fc4b3bbc16332430fa8fec1d4b07d20014aded41b3e54307215f4f7a6d390eb`. Installed files are byte-identical to the verified build/source copies.
- Actual dragging across multiple +12 blink transitions, result-animation expiry and inventory boundaries, plus new/existing enhancer 3×3 occupancy, still requires in-game acceptance after restart.

## 2.1.0 independent module enhancer

- Added the independent `module_crafter_enhancer` with its verified 48×48 anvil texture, shared material grid, instant daytime enhancement, stat/cap/failure previews and atomic scrap exchange. The crafter no longer exposes the enhancement tab; legacy slot contents are returned to the store inventory on load.
- The enhancer costs 400. Jackson uses the existing 70% unowned / 15% owned machine-stock chances after the old stock rolls; the retired merchant always lists one. Rules assertions cover item mapping, unlocks, probability boundaries, merchant quantity, craft-material exclusion and vanilla-cap preview boundaries.
- Preserved the user-confirmed 2.0.6 success-rate table, 120 ms result frames, T-shaped stabilizing clip and damaged/ruined-only common housing rules; the older conflicting values in the new handoff tables were not restored.
- Rules tests passed with `All rules assertions passed`. Build and local installation passed; the installed manifest is 2.1.0 and still declares `reflection-external`.
- Official scan: `Refused: False`, `MethodsScanned: 399`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Installed DLL SHA256: `b09662efdf1a32207d9751a54ec35e9de502611a36b59e4f9329213dbd84f297`; manifest SHA256: `0e536a42441da24472f241ee65a32163b0ebdf61132b5cc8e0df3ba17e9f992f`; enhancer PNG SHA256: `720672226d33827ff2350d6b6197933ca792984a7a9b0f99bab4cc0b33d2699a`. Each installed file is byte-identical to its verified build/source counterpart.
- No post-install game restart has yet been supplied for this version. Actual initialization, old-slot migration, machine/window appearance, material isolation, shop stocking, transactions, animations, save/load and original crafting regressions remain pending in-game acceptance.

## 2.0.6 material, clip-shape and success-rate update

- User confirmed that enhancement animation playback is working in 2.0.5. The separate badge-placement acceptance remains unconfirmed.
- Common recipes no longer fall back from damaged/ruined housings to intact modules, and the former intact-housing +5% bonus is removed. Explicit intact-module inputs for advanced recipes are unchanged.
- The stabilizing clip now uses explicit `111010` data on a three-column grid and a matching 48×32 T-shaped texture. Pixel checks passed: all three top-cell centers and the lower-middle center are opaque; both lower side centers are transparent.
- From +7 onward, enhancement success rates are the original values plus 30 percentage points: 80%, 70%, 60%, 55%, 45% for +7→+8 through +11→+12. Other levels and failure rules are unchanged.
- Rules tests passed, including rejected intact-only common-material plans, accepted damaged/ruined plans, unchanged advanced intact-module inputs and the complete updated success-rate table.
- Build and installation passed after the user closed the game. The installed manifest is 2.0.6; the installed DLL, manifest and stabilizing-clip PNG are byte-identical to the verified build/source copies.
- Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Material UI behavior, clip occupancy and displayed chances still require in-game acceptance; build and scan results are not gameplay verification.

## 2.0.5 animation scheduler and T-shape hotfix

- Runtime result for 2.0.4: the log confirmed `Initialized: 模组制造机 v2.0.4` and successful enhancement transactions, but the user observed a frozen effect frame and the badge still outside the artwork.
- Confirmed scheduler root cause: `OnEnable` runs before `LoadedMod.IsEnabled` is assigned. `ModHelper.Delay` therefore returns `null`; the old visual scheduler still set `_running=true`, preventing every later retry. Only direct item validations changed the displayed effect frame.
- Lifecycle fix: start the visual scheduler from `OnGameLoadedLate`, unregister it on disable, and reset `_running` when the official delay request returns `null` so a later start can retry.
- Confirmed atlas dimensions: vanilla L sprites are 48×32, while T sprites are 32×48. The old code treated both L and T as 48 pixels wide and T as 32 pixels high, displacing T badges and effect centers by 16 pixels.
- Rules tests: passed. Build and install: passed after the user closed the game. Installed manifest is 2.0.5; installed DLL and manifest are byte-identical to the verified build/source copies.
- Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- The user subsequently confirmed that enhancement animation playback is working. Effect cleanup and corrected badge placement still require separate user confirmation.

## 2.0.4 visual feedback hotfix

- User-reported symptoms in 2.0.3: success/failure animations were not perceptible, and low-level badges were too far from the module artwork.
- Evidence: enhancement results and all animation textures were present in the runtime log. The original timing was only 50 ms per frame, including a transparent final frame, and rapid confirmations replace the preceding animation.
- Changes: result animations now use 120 ms per frame with the original frame counts and interruption behavior; +1 through +8 badges are inset 4 pixels left and 4 pixels down from the previous grid-edge position.
- Rules tests: passed with the updated timing assertion. Build and install: passed after the user closed the game. Installed manifest is 2.0.4; installed DLL and manifest are byte-identical to the verified build/source copies.
- Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same declared evaporator reflection findings).
- Actual animation visibility, frame pacing and badge placement still require user confirmation after restarting; build and scan results are not visual acceptance.

## 2.0.3 initialization hotfix

- Logged root cause in 2.0.2: the unrelated static callback `<>c::<Evaporator>b__0_0` has no captured `machine` field, but the transpiler still threw `Evaporator closure no longer captures machine` instead of passing it through.
- Fix: closures without a `machine` field and methods without the intended quality-cap call site now return unchanged. Only a successful replacement logs the patched method name.
- Rules tests: passed. Build and install: passed. Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2`; installed DLL and manifest match the verified copies.
- Runtime verification after a full restart: `Initialized: 模组制造机 v2.0.3`, `Evaporator quality cap patched in <Evaporator>b__7.`, `Enabled`, and audit `running=True`.
- The restarted log has no `Init failed`, `HarmonyException`, other exception/error entries, Quantum Console activation error, missing `module_crafter` item warnings or child-item load failures. The initialization cause of the Quantum Console problem is resolved.
- Gameplay acceptance remains separate and is still required as listed below; initialization evidence alone does not verify enhancement rules, UI, saves or evaporator behavior during play.

## 2.0.2 initialization hotfix

- Symptom: the game's Quantum Console command input appeared after activation; the mod was not actually running.
- Logged root cause: `Evaporator quality cap call site was not found` in `<Evaporator>b__2`. The selector included unrelated three-parameter callbacks before the real night callback.
- Current-game IL verification: the quality calculation is in `<Evaporator>b__7`; its first `Math.Clamp(int,int,int)` quality cap is preceded by `ldc.i4.s 100`. The later liquid-distribution clamp is separate.
- Fix: nonmatching closures pass through unchanged; a successful replacement logs `Evaporator quality cap patched in <method>`.
- Rules tests: passed. Build and install: passed. Installed manifest is 2.0.2 with `reflection-external`; installed DLL and manifest match the verified build/source copies.
- Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2`.
- Runtime initialization and the successful `b__7` patch log still require the next game restart. The earlier missing-item/dummy warnings were consequences of initialization failing; the affected session should not be saved over the original save.

## 2.0.1 activation hotfix

- Root cause: the game refused activation because scanner capability `reflection-external` was not declared in `manifest.xml`.
- Fix: version 2.0.1 declares `<Capability>reflection-external</Capability>`; no gameplay rules changed.
- Rules tests: passed. Build: passed; DLL size 104960 bytes.
- Official scan: `Refused: False`, `MethodsScanned: 366`, `Capabilities: reflection-external`, `HardFlags: 0`, `Findings: 2` (the same two narrow evaporator reflection findings).
- Source manifest verification: version 2.0.1 and the required capability are present.
- Install: passed after the user closed the game. Installed manifest is version 2.0.1 with `reflection-external`; installed DLL and manifest are byte-identical to the verified source/build copies. Actual in-game activation still requires the user to restart and approve the declared capability if prompted.

## 2.0.0 build

- Version: 2.0.0. Target game: 0.46D. Manifest ID remains `module_crafter`; Workshop ID remains `3840773884`.
- Rules tests: passed with `All rules assertions passed` (`python Modding/ModuleCrafter/run_tests.py`). Coverage includes the original crafting rules and all enhancement F/G values, costs, success rates, failure results, prices, learning limits, soft caps, exchange and source chances.
- Build: passed (`python Modding/ModuleCrafter/build.py`). Final DLL size: 104960 bytes.
- Install: `Mods/ModuleCrafter/`; the final installed DLL was byte-identical to `Modding/ModuleCrafter/build/ModuleCrafter.dll`.
- Assets: 219 enhancement PNGs are available in the flat runtime texture mapping. All 94 level/overlay/blink/animation paths generated by the runtime naming rules exist; no missing files were found.
- F9 debug console remains disabled for 2.0.0 and is only registered by 0.x development versions.

## Official scan

Command: `python Modding/ModuleCrafter/run_scan.py`.

- `Refused: False`
- `MethodsScanned: 366`
- `Capabilities: reflection-external`
- `HardFlags: 0`
- `Findings: 2`
- Finding 1: `reflection-external: ModuleCrafterEvaporatorPatch.Transpiler -> Type.GetField on a computed type`.
- Finding 2: `reflection-external: ModuleCrafterEnhanceSources` has no finding; the second finding is `ModuleCrafterEvaporatorPatch/<TargetMethods>d__0.MoveNext -> Type.GetMethods on a computed type`.

Both findings belong to the narrow evaporator closure lookup/field access used by the Harmony transpiler. They are reported as findings, not hard flags or refusal.

## Verification boundary

Compilation, pure-rule assertions, asset presence and scanner output do not prove gameplay correctness. The following still require in-game acceptance:

1. Nine module types from +0 through +12, including all five result branches, costs, stacking consumption, catalyst refund, clip reset and rapid-click/interrupt behavior.
2. Backpack, tooltip, enhancement panel and installed-slot visuals at +0, +1, +8, +9 and +12.
3. Learning algorithm, degradation, overvolt, premium/inferior and all shapes across save/load and disable/enable restoration.
4. Retired merchant daily cache, each additional seller chance, stolen flags and the single scavenging bonus without scanner-double-roll duplication.
5. Crafting preview/final soft-cap agreement and the evaporator quality-cap change without altering liquid distribution.
6. Chinese/English layout and legacy 1.0 crafting, cancellation, refund, morning-report and machine-targeting regressions.

Version 2.1.6 was uploaded to the existing public Workshop item without changing its visibility. Git commit and push state are recorded by the repository history.
