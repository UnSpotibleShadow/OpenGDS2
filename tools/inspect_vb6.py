#!/usr/bin/env python3
"""Static Visual Basic 5/6 project inspector for GameDev2DX.exe.

This tool never executes the target. It follows the VB6 metadata graph embedded in
compiled executables:

    VBHeader -> ProjectInfo -> ObjectTable -> Object/ObjectInfo -> controls/events

It intentionally focuses on structural recovery useful to OpenGDS2 rather than
trying to be a complete VB6 decompiler.
"""
from __future__ import annotations

import argparse
import csv
import json
import struct
from dataclasses import dataclass
from pathlib import Path
from typing import Any


class ParseError(RuntimeError):
    pass


@dataclass(frozen=True)
class Section:
    name: str
    rva: int
    virtual_size: int
    raw_offset: int
    raw_size: int


class PeImage:
    def __init__(self, data: bytes):
        self.data = data
        if data[:2] != b"MZ":
            raise ParseError("not a PE/MZ executable")
        pe = self.u32_off(0x3C)
        if data[pe:pe+4] != b"PE\0\0":
            raise ParseError("PE signature not found")
        number_of_sections = self.u16_off(pe + 6)
        optional_size = self.u16_off(pe + 20)
        optional = pe + 24
        magic = self.u16_off(optional)
        if magic != 0x10B:
            raise ParseError(f"expected PE32 optional header, got magic 0x{magic:04X}")
        self.image_base = self.u32_off(optional + 28)
        self.entry_rva = self.u32_off(optional + 16)
        section_table = optional + optional_size
        sections: list[Section] = []
        for i in range(number_of_sections):
            off = section_table + i * 40
            name = data[off:off+8].split(b"\0", 1)[0].decode("ascii", "replace")
            virtual_size, rva, raw_size, raw_offset = struct.unpack_from("<IIII", data, off + 8)
            sections.append(Section(name, rva, virtual_size, raw_offset, raw_size))
        self.sections = sections

    def u16_off(self, off: int) -> int:
        return struct.unpack_from("<H", self.data, off)[0]

    def s16_off(self, off: int) -> int:
        return struct.unpack_from("<h", self.data, off)[0]

    def u32_off(self, off: int) -> int:
        return struct.unpack_from("<I", self.data, off)[0]

    def va_to_offset(self, va: int) -> int:
        rva = va - self.image_base
        for s in self.sections:
            # Zero-filled virtual memory has no file data; BSS must not map into PE headers.
            if s.rva <= rva < s.rva + s.raw_size:
                off = s.raw_offset + (rva - s.rva)
                if off >= len(self.data):
                    break
                return off
        raise ParseError(f"VA 0x{va:08X} is not file-backed")

    def u16(self, va: int) -> int:
        return struct.unpack('<H', self.bytes_at(va, 2))[0]

    def s16(self, va: int) -> int:
        return struct.unpack('<h', self.bytes_at(va, 2))[0]

    def u32(self, va: int) -> int:
        return struct.unpack('<I', self.bytes_at(va, 4))[0]

    def nts(self, va: int, encoding: str = "cp932", max_len: int = 4096) -> str:
        if va == 0:
            return ""
        off = self.va_to_offset(va)
        end = self.data.find(b"\0", off, min(len(self.data), off + max_len))
        if end < 0:
            end = min(len(self.data), off + max_len)
        return self.data[off:end].decode(encoding, "replace")

    def bytes_at(self, va: int, size: int) -> bytes:
        if size < 0:
            raise ParseError('negative byte count')
        off = self.va_to_offset(va)
        section = next(s for s in self.sections if s.rva <= va - self.image_base < s.rva + s.raw_size)
        if size > section.raw_size - (va - self.image_base - section.rva) or off + size > len(self.data):
            raise ParseError(f'read crosses file-backed section at VA 0x{va:08X}')
        return self.data[off:off+size]


def hexva(v: int | None) -> str:
    return "" if v is None else f"0x{v:08X}"


def classify_object(flags: int) -> str:
    # VB6 object flag families documented by Semi-VB-Decompiler. Keep this
    # deliberately conservative: the labels are aids, not ABI assertions.
    low = flags & 0xFF
    if low & 0x80:
        return "form_or_designer"
    if low in (0x01, 0x21):
        return "module"
    if low in (0x03, 0x23):
        return "class_or_form"
    return "object"


def find_vb_header(pe: PeImage) -> int:
    # Prefer the metadata signature rather than trusting the entry stub pattern.
    candidates: list[int] = []
    start = 0
    while True:
        off = pe.data.find(b"VB5!", start)
        if off < 0:
            break
        for s in pe.sections:
            if s.raw_offset <= off < s.raw_offset + s.raw_size:
                va = pe.image_base + s.rva + (off - s.raw_offset)
                candidates.append(va)
                break
        start = off + 1
    if not candidates:
        raise ParseError("VB5! header signature not found")
    # A genuine VBHeader has a plausible ProjectInfo pointer at +0x30.
    for va in candidates:
        try:
            project = pe.u32(va + 0x30)
            _ = pe.u32(project + 0x04)
            return va
        except Exception:
            pass
    raise ParseError("VB5! signature found, but no valid ProjectInfo pointer")


def proc_descriptor(pe: PeImage, code_info: int) -> dict[str, Any]:
    """Decode the P-Code ProcDscInfo prefix used by this executable.

    Semi-VB-Decompiler models the first fields as table, flags, frame size and
    procedure size. For P-Code, procedure_start = CodeInfo - ProcSize.
    """
    if not code_info:
        return {}
    try:
        table = pe.u32(code_info)
        flag4 = pe.u16(code_info + 4)
        frame_size = pe.u16(code_info + 6)
        proc_size = pe.u16(code_info + 8)
        start = code_info - proc_size
        # A small preview is enough for evidence/repeatability without copying
        # whole procedures into generated reports.
        preview = pe.bytes_at(start, min(proc_size, 32)).hex(" ") if proc_size else ""
        return {
            "code_info": hexva(code_info),
            "object_info": hexva(table),
            "flags_4": flag4,
            "frame_size": frame_size,
            "proc_size": proc_size,
            "start": hexva(start),
            "preview_hex": preview,
        }
    except Exception as exc:
        return {"code_info": hexva(code_info), "error": str(exc)}


def read_controls(pe: PeImage, object_info: int, expected_object_info: int) -> list[dict[str, Any]]:
    # ObjectInfo is 0x38 bytes. OptionalObjectInfo immediately follows for forms.
    optional = object_info + 0x38
    control_count = pe.u32(optional + 0x20)
    control_array = pe.u32(optional + 0x24)
    if control_count > 2048:
        raise ParseError(f"implausible control count {control_count}")
    controls: list[dict[str, Any]] = []
    for i in range(control_count):
        cva = control_array + i * 0x28
        event_count = pe.u16(cva + 0x02)
        index = pe.s16(cva + 0x0C)
        event_table = pe.u32(cva + 0x18)
        name_ptr = pe.u32(cva + 0x20)
        name = pe.nts(name_ptr) if name_ptr else ""
        event_slots: list[dict[str, Any]] = []
        if event_table and event_count <= 512:
            # tEventTable is 0x18 bytes followed by EventCount pointers to
            # tEventPointer records. tEventPointer.aEvent is at +0x0A.
            try:
                table_object_info = pe.u32(event_table + 0x08)
                if table_object_info == expected_object_info:
                    for slot in range(event_count):
                        event_pointer = pe.u32(event_table + 0x18 + slot * 4)
                        if not event_pointer:
                            continue
                        code_info = pe.u32(event_pointer + 0x0A)
                        event_slots.append({
                            "slot": slot,
                            "event_pointer": hexva(event_pointer),
                            "code_info": hexva(code_info),
                            "procedure": proc_descriptor(pe, code_info),
                        })
            except Exception:
                pass
        controls.append({
            "ordinal": i,
            "address": hexva(cva),
            "name": name,
            "index": index,
            "event_count": event_count,
            "event_table": hexva(event_table),
            "events": event_slots,
        })
    return controls


def inspect(path: Path) -> dict[str, Any]:
    pe = PeImage(path.read_bytes())
    vb_header = find_vb_header(pe)
    project_info = pe.u32(vb_header + 0x30)
    object_table = pe.u32(project_info + 0x04)
    native_code = pe.u32(project_info + 0x20)
    object_count = pe.u16(object_table + 0x2A)
    compiled_count = pe.u16(object_table + 0x2C)
    objects_in_use = pe.u16(object_table + 0x2E)
    object_array = pe.u32(object_table + 0x30)

    objects: list[dict[str, Any]] = []
    for i in range(object_count):
        ova = object_array + i * 0x30
        object_info = pe.u32(ova + 0x00)
        object_name_ptr = pe.u32(ova + 0x18)
        proc_count = pe.u32(ova + 0x1C)
        proc_names = pe.u32(ova + 0x20)
        object_type = pe.u32(ova + 0x28)
        name = pe.nts(object_name_ptr) if object_name_ptr else f"object_{i}"

        entry: dict[str, Any] = {
            "ordinal": i,
            "address": hexva(ova),
            "name": name,
            "kind": classify_object(object_type),
            "object_type": f"0x{object_type:08X}",
            "object_info": hexva(object_info),
            "procedure_count": proc_count,
            "procedure_names_array": hexva(proc_names),
        }

        if object_info:
            try:
                oi_count = pe.u32(object_info + 0x20)
                proc_table = pe.u32(object_info + 0x24)
                constant_pool = pe.u32(object_info + 0x34)
                entry["object_info_procedure_count"] = oi_count
                entry["procedure_table"] = hexva(proc_table)
                entry["constant_pool"] = hexva(constant_pool)
                procedures: list[dict[str, Any]] = []
                if proc_table and oi_count <= 4096:
                    for pi in range(oi_count):
                        code_info = pe.u32(proc_table + pi * 4)
                        if code_info:
                            pd = proc_descriptor(pe, code_info)
                            pd["ordinal"] = pi
                            procedures.append(pd)
                entry["procedures"] = procedures
            except Exception as exc:
                entry["object_info_error"] = str(exc)

        if object_info and (object_type & 0x80):
            try:
                optional = object_info + 0x38
                entry["optional_object_info"] = {
                    "address": hexva(optional),
                    "control_count": pe.u32(optional + 0x20),
                    "control_array": hexva(pe.u32(optional + 0x24)),
                    "event_count": pe.u16(optional + 0x28),
                    "pcode_count_or_flags": pe.u16(optional + 0x2A),
                    "initialize_event_offset": pe.u16(optional + 0x2C),
                    "terminate_event_offset": pe.u16(optional + 0x2E),
                    "event_link_array": hexva(pe.u32(optional + 0x30)),
                }
                entry["controls"] = read_controls(pe, object_info, object_info)
            except Exception as exc:
                entry["controls_error"] = str(exc)

        objects.append(entry)

    return {
        "source": str(path),
        "pe": {
            "image_base": hexva(pe.image_base),
            "entry_point": hexva(pe.image_base + pe.entry_rva),
            "sections": [s.__dict__ for s in pe.sections],
        },
        "vb6": {
            "vb_header": hexva(vb_header),
            "runtime_signature": pe.bytes_at(vb_header, 4).decode("ascii", "replace"),
            "language_dll": pe.bytes_at(vb_header + 6, 14).split(b"\0", 1)[0].decode("ascii", "replace"),
            "project_info": hexva(project_info),
            "object_table": hexva(object_table),
            "compile_mode": "p-code" if native_code == 0 else "native",
            "native_code_pointer": hexva(native_code),
            "object_count": object_count,
            "compiled_object_count": compiled_count,
            "objects_in_use": objects_in_use,
            "object_array": hexva(object_array),
        },
        "objects": objects,
    }


def write_outputs(report: dict[str, Any], out_dir: Path) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)
    (out_dir / "vb6_project.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8"
    )

    with (out_dir / "vb6_objects.tsv").open("w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter="\t")
        w.writerow(["ordinal", "name", "kind", "object_type", "object_info", "procedure_count", "control_count"])
        for o in report["objects"]:
            w.writerow([
                o["ordinal"], o["name"], o["kind"], o["object_type"], o["object_info"],
                o["procedure_count"], o.get("optional_object_info", {}).get("control_count", ""),
            ])

    # Focus reports are convenient diffs for the startup path we're reconstructing.
    by_name = {o["name"]: o for o in report["objects"]}
    for name in ("fGMGameNew", "fGMGameKaishi", "fGMSKoyou", "fGMSKoyouKz"):
        if name in by_name:
            (out_dir / f"vb6_{name}.json").write_text(
                json.dumps(by_name[name], ensure_ascii=False, indent=2), encoding="utf-8"
            )

    summary = []
    vb = report["vb6"]
    summary.append("OpenGDS2 static VB6 inspection\n")
    summary.append(f"Compile mode: {vb['compile_mode']}")
    summary.append(f"VBHeader: {vb['vb_header']}")
    summary.append(f"ProjectInfo: {vb['project_info']}")
    summary.append(f"ObjectTable: {vb['object_table']}")
    summary.append(f"Objects: {vb['object_count']}")
    summary.append("")
    for name in ("fGMGameNew", "fGMGameKaishi", "fGMSKoyou", "fGMSKoyouKz"):
        obj = by_name.get(name)
        if not obj:
            continue
        summary.append(f"[{name}] procedures={obj['procedure_count']} controls={len(obj.get('controls', []))}")
        for c in obj.get("controls", []):
            if c["events"]:
                for ev in c["events"]:
                    summary.append(
                        f"  {c['name']} index={c['index']} event_slot={ev['slot']} -> {ev['code_info']} "
                        f"start={ev['procedure'].get('start','')} size={ev['procedure'].get('proc_size','')}"
                    )
        summary.append("")
    (out_dir / "vb6_startup_map.txt").write_text("\n".join(summary) + "\n", encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("exe", type=Path, help="VB6 PE executable to inspect")
    ap.add_argument("--out", type=Path, default=Path("generated/vb6"), help="output directory")
    args = ap.parse_args()
    report = inspect(args.exe)
    write_outputs(report, args.out)
    print(json.dumps(report["vb6"], ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
