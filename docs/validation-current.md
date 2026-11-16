# ModuleCrafter validation (current)

Date: 2026-11-16 Asia/Shanghai

- Version: 1.0.0 (first official freeze). Target game 0.46D.
- Rules tests: passed (`python Modding/ModuleCrafter/run_tests.py` from the game root).
- Build: compiled `Modding/ModuleCrafter/build/ModuleCrafter.dll` (70144 bytes). Install to `Mods/ModuleCrafter/` was blocked because the game was open (WinError 1224); install still needs a closed game.
- Official scan of the 1.0.0 build DLL: MethodsScanned 237, Capabilities none, HardFlags 0, Findings 0, Refused False.
- F9 debug console is disabled for 1.0.0 (enabled only on 0.x versions).
- Do not treat compile/scan as proof of gameplay correctness. Latest ticket, morning-report, and special-effect changes still await in-game acceptance.
- Not in this freeze: crate, scrapyard, thief, underground, and smuggler live stock hooks; 9 extra locales; Workshop publish.
