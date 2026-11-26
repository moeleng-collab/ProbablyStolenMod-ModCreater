from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[2]
project = Path(__file__).resolve().parent
tests = project / "tests"
dotnet = root / ".port-tools" / "dotnet" / "dotnet.exe"
csc = root / ".port-tools" / "dotnet" / "sdk" / "8.0.427" / "Roslyn" / "bincore" / "csc.dll"
refs = root / ".port-tools" / "dotnet" / "packs" / "Microsoft.NETCore.App.Ref" / "8.0.33" / "ref" / "net8.0"
output = tests / "RulesTests.dll"
args = [
    str(dotnet),
    str(csc),
    "-nologo",
    "-target:exe",
    "-langversion:latest",
    "-nullable:enable",
    f"-out:{output}",
]
args.extend(f"-reference:{path}" for path in refs.glob("*.dll"))
args.extend([
    str(project / "src" / "ModuleCrafterRules.cs"),
    str(project / "src" / "ModuleCrafterExpansion.cs"),
    str(project / "src" / "ModuleCrafterEnhanceRules.cs"),
    str(project / "src" / "ModuleCrafterSpecialEffectRules.cs"),
    str(tests / "RulesTests.cs"),
    str(tests / "ExpansionRulesTests.cs"),
    str(tests / "EnhanceRulesTests.cs"),
])
subprocess.run(args, check=True, cwd=root)

runtimeconfig = output.with_suffix(".runtimeconfig.json")
if not runtimeconfig.exists():
    runtimeconfig.write_text(
        '{"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.33"}}}\n',
        encoding="utf-8",
    )

subprocess.run([str(dotnet), str(output)], check=True, cwd=tests)
