from pathlib import Path
import shutil
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
project = Path(__file__).resolve().parent
dev_mode = "--dev" in sys.argv
sdk = root / ".port-tools" / "dotnet" / "sdk" / "8.0.427"
csc = sdk / "Roslyn" / "bincore" / "csc.dll"
managed = root / "Probably Stolen_Data" / "Managed"
out_dir = project / "build"
out_dir.mkdir(exist_ok=True)
out = out_dir / "ModuleCrafter.dll"

args = [
    str(root / ".port-tools" / "dotnet" / "dotnet.exe"),
    str(csc),
    "-nologo",
    "-target:library",
    "-platform:anycpu",
    "-langversion:latest",
    "-nullable:enable",
    "-optimize+",
    "-deterministic+",
    f"-out:{out}",
    f"-pdb:{out_dir / 'ModuleCrafter.pdb'}",
    "-debug:portable",
]
if dev_mode:
    args.append("-define:MC_DEBUG")
args.extend(f"-reference:{path}" for path in sorted(managed.glob("*.dll")))
sources = sorted((project / "src").glob("*.cs"))
if not dev_mode:
    sources = [path for path in sources if path.name != "ModuleCrafterDebug.cs"]
args.extend(str(path) for path in sources)
args.append(str(project / "Properties" / "AssemblyInfo.cs"))
subprocess.run(args, check=True, cwd=root)

install = root / "Mods" / "ModuleCrafter"
install.mkdir(parents=True, exist_ok=True)
shutil.copy2(out, install / out.name)
if dev_mode:
    manifest_text = (project / "manifest.xml").read_text(encoding="utf-8")
    if "<Capability>files</Capability>" not in manifest_text:
        if "<Capabilities />" in manifest_text:
            manifest_text = manifest_text.replace(
                "<Capabilities />",
                "<Capabilities>\n    <Capability>files</Capability>\n  </Capabilities>",
            )
        else:
            manifest_text = manifest_text.replace(
                "<Capabilities>",
                "<Capabilities>\n    <Capability>files</Capability>",
                1,
            )
    dev_manifest = out_dir / "manifest.dev.xml"
    dev_manifest.write_text(manifest_text, encoding="utf-8")
    shutil.copy2(dev_manifest, install / "manifest.xml")
    debug_on = project / "debug.on"
    if debug_on.exists():
        shutil.copy2(debug_on, install / "debug.on")
else:
    shutil.copy2(project / "manifest.xml", install / "manifest.xml")
    (install / "debug.on").unlink(missing_ok=True)

textures_src = project / "Textures"
textures_dst = install / "Textures"
if textures_src.exists():
    if textures_dst.exists():
        shutil.rmtree(textures_dst)
    has_files = any(textures_src.rglob("*.png")) or any(textures_src.rglob("*.jpg"))
    if has_files:
        shutil.copytree(textures_src, textures_dst)
sounds_src = project / "Sounds"
sounds_dst = install / "Sounds"
if sounds_dst.exists():
    shutil.rmtree(sounds_dst)
if sounds_src.exists() and any(sounds_src.glob("*.wav")):
    shutil.copytree(sounds_src, sounds_dst)
print(f"Built and installed ({'development' if dev_mode else 'release'}): {install}")
