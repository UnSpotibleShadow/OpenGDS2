#!/usr/bin/env python3
"""Apply hash-checked artwork edits to a separate local override directory."""
import argparse
import hashlib
import io
import json
from pathlib import Path, PurePosixPath
import re

from PIL import Image, ImageDraw, ImageFont


def graphic_path(root, relative):
    if not isinstance(relative, str) or "\\" in relative or ":" in relative or "\0" in relative:
        raise ValueError("Expected a relative Graphic/... BMP path")
    parts = relative.split("/")
    if len(parts) < 2 or parts[0] != "Graphic" or any(part in {"", ".", ".."} for part in parts):
        raise ValueError("Artwork paths must stay in Graphic/")
    path = root.joinpath(*parts)
    if PurePosixPath(relative).suffix.lower() != ".bmp" or not path.resolve().is_relative_to(root):
        raise ValueError("Only local imported BMP graphics can be patched")
    for parent in [path, *path.parents]:
        if parent == root:
            break
        if parent.is_symlink():
            raise ValueError("Symlink artwork paths are not supported")
    return path


def color(value):
    if not isinstance(value, str) or not re.fullmatch(r"#[0-9a-fA-F]{6}", value):
        raise ValueError("Colors must use #RRGGBB")
    return value


def patch_artwork(original, overrides, plan):
    original, overrides = Path(original).resolve(), Path(overrides).resolve()
    if overrides.is_relative_to(original):
        raise ValueError("Overrides must be separate from the imported original directory")
    catalog = json.loads((original / "catalog.json").read_text(encoding="utf-8"))
    if (plan.get("format") != "OpenGDS2LocalArtworkPatch" or plan.get("schemaVersion") != 1 or
            not re.fullmatch(r"[0-9a-f]{64}", str(plan.get("sourceHash", ""))) or
            plan["sourceHash"] != catalog.get("SourceHash")):
        raise ValueError("The patch plan must match a locally imported game catalog")
    entries = plan.get("patches")
    if not isinstance(entries, list) or not 1 <= len(entries) <= 16:
        raise ValueError("Expected between 1 and 16 bitmap patches")
    prepared = {}
    for entry in entries:
        source = graphic_path(original, entry.get("path"))
        target = graphic_path(overrides, entry.get("path")).with_suffix(".png")
        if target in prepared or target.exists() or target.is_symlink():
            raise ValueError("An override already exists or the plan repeats a target")
        data = source.read_bytes()
        if hashlib.sha256(data).hexdigest() != entry.get("sha256"):
            raise ValueError("Original bitmap hash does not match the patch plan")
        with Image.open(io.BytesIO(data)) as bitmap:
            if bitmap.format != "BMP" or list(bitmap.size) != entry.get("size") or bitmap.width * bitmap.height > 4_000_000:
                raise ValueError("Unexpected original bitmap format or dimensions")
            image = bitmap.convert("RGB")
        operations = entry.get("operations")
        if not isinstance(operations, list) or not 1 <= len(operations) <= 256:
            raise ValueError("Expected between 1 and 256 rectangle operations")
        draw = ImageDraw.Draw(image)
        for operation in operations:
            rect = operation.get("rect")
            if not isinstance(rect, list) or len(rect) != 4 or any(type(value) is not int for value in rect):
                raise ValueError("Rectangle must contain four integer coordinates")
            x, y, width, height = rect
            if x < 0 or y < 0 or width <= 0 or height <= 0 or x + width > image.width or y + height > image.height:
                raise ValueError("Patch rectangle exceeds the original bitmap")
            background = color(operation.get("background"))
            text = operation.get("text", "")
            if not isinstance(text, str) or len(text) > 128 or any(ord(letter) < 32 or ord(letter) > 126 for letter in text):
                raise ValueError("Built-in patch lettering supports printable English ASCII")
            if text:
                size = operation.get("fontSize", 12)
                if type(size) is not int or not 6 <= size <= 48:
                    raise ValueError("Font size must be between 6 and 48")
                font = ImageFont.load_default(size=size)
                left, top, right, bottom = draw.textbbox((0, 0), text, font=font)
                if right - left > width or bottom - top > height:
                    raise ValueError("Translated text does not fit the patch rectangle")
                foreground = color(operation.get("color", "#000000"))
            draw.rectangle((x, y, x + width - 1, y + height - 1), fill=background)
            if text:
                draw.text((x + (width - right + left) // 2 - left,
                           y + (height - bottom + top) // 2 - top), text, font=font, fill=foreground)
        encoded = io.BytesIO()
        image.save(encoded, format="PNG")
        prepared[target] = encoded.getvalue()
    # Validation/rendering of every entry precedes any override write.
    written = []
    try:
        for target, data in prepared.items():
            target.parent.mkdir(parents=True, exist_ok=True)
            with target.open("xb") as file:
                written.append(target)
                file.write(data)
    except Exception:
        for target in written:
            target.unlink(missing_ok=True)
        raise
    return list(prepared)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--original", type=Path, required=True)
    parser.add_argument("--overrides", type=Path)
    action = parser.add_mutually_exclusive_group(required=True)
    action.add_argument("--plan", type=Path)
    action.add_argument("--inspect", help="Print local bitmap metadata for creating a patch plan")
    args = parser.parse_args()
    if args.plan and args.overrides is None:
        parser.error("--overrides is required when applying a plan")
    try:
        if args.inspect:
            root = args.original.resolve()
            source = graphic_path(root, args.inspect)
            catalog = json.loads((root / "catalog.json").read_text(encoding="utf-8"))
            with Image.open(source) as bitmap:
                if bitmap.format != "BMP":
                    raise ValueError("Expected an imported BMP graphic")
                size = list(bitmap.size)
            print(json.dumps({"format": "OpenGDS2LocalArtworkPatch", "schemaVersion": 1,
                              "sourceHash": catalog["SourceHash"], "patches": [{
                                  "path": args.inspect, "sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                                  "size": size, "operations": []}]}, indent=2))
            parser.exit()
        outputs = patch_artwork(args.original, args.overrides,
                                json.loads(args.plan.read_text(encoding="utf-8")))
    except (OSError, ValueError, KeyError, TypeError, AttributeError) as error:
        parser.exit(1, f"Local artwork patch failed: {error}\n")
    print(f"Created {len(outputs)} local overrides; imported original files were not changed.")
