from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
TARGET = ROOT / "Textures" / "Items" / "module_crafter_safeguard_clip.png"

image = Image.new("RGBA", (48, 32), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)

outline = (48, 32, 27, 255)
deep = (66, 43, 34, 255)
shadow = (88, 57, 40, 255)
metal = (126, 84, 51, 255)
warm = (153, 105, 61, 255)
highlight = (196, 149, 91, 255)
glint = (224, 186, 126, 255)
slot = (54, 40, 34, 255)

# Pixel silhouette of a T: a three-cell top bar and a centered lower stem.
silhouette = [
    (3, 4), (44, 4), (44, 6), (46, 6), (46, 15), (44, 15),
    (44, 17), (30, 17), (30, 27), (28, 27), (28, 30),
    (19, 30), (19, 27), (17, 27), (17, 17), (3, 17),
    (3, 15), (1, 15), (1, 6), (3, 6),
]
draw.polygon(silhouette, fill=outline)
draw.rectangle((3, 6, 44, 15), fill=shadow)
draw.rectangle((4, 6, 43, 8), fill=highlight)
draw.rectangle((4, 9, 43, 13), fill=metal)
draw.rectangle((4, 14, 43, 15), fill=deep)
draw.rectangle((5, 6, 42, 6), fill=glint)
draw.line((4, 9, 4, 13), fill=warm)
draw.line((43, 9, 43, 13), fill=deep)

# Centered clamp stem, kept wholly inside the lower-middle inventory cell.
draw.rectangle((19, 16, 28, 27), fill=shadow)
draw.rectangle((20, 16, 26, 25), fill=metal)
draw.line((20, 16, 20, 25), fill=highlight)
draw.line((21, 16, 21, 24), fill=warm)
draw.line((27, 17, 27, 26), fill=deep)
draw.rectangle((20, 26, 27, 28), fill=deep)
draw.line((21, 26, 25, 26), fill=highlight)
draw.point((20, 29), fill=highlight)

# Three top fasteners and one stem fastener make the silhouette read as a tool.
for x in (8, 24, 40):
    draw.rectangle((x - 2, 9, x + 2, 12), fill=deep)
    draw.point((x - 1, 9), fill=highlight)
    draw.line((x - 1, 11, x + 1, 10), fill=slot)

draw.rectangle((22, 19, 25, 22), fill=deep)
draw.point((22, 19), fill=highlight)
draw.line((22, 21, 24, 20), fill=slot)

# Small jaws at the ends reinforce the clamp silhouette without entering other rows.
draw.rectangle((1, 8, 2, 13), fill=warm)
draw.point((2, 7), fill=glint)
draw.rectangle((45, 8, 45, 13), fill=deep)

image.save(TARGET, optimize=True)

check = Image.open(TARGET).convert("RGBA")
assert check.size == (48, 32)
expected = {
    (8, 8): True,
    (24, 8): True,
    (40, 8): True,
    (8, 24): False,
    (24, 24): True,
    (40, 24): False,
}
for point, opaque in expected.items():
    alpha = check.getpixel(point)[3]
    assert (alpha > 0) == opaque, (point, alpha, opaque)
    print(f"{point}: alpha={alpha}")
print(f"Saved {TARGET} ({TARGET.stat().st_size} bytes)")
