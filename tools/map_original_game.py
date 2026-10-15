#!/usr/bin/env python3
"""Rebuild the supported original's structural map without executing it.

Only addresses, sizes, hashes and technical names are emitted. Original strings,
instructions, artwork and private paths are deliberately excluded. The structure
file supplies previously recovered serialized-record boundaries and the opcode
length schema; each boundary is revalidated against the pinned executable.
"""
from __future__ import annotations

import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import struct
import sys

from inspect_vb6 import PeImage, ParseError, inspect
import trace_hiring_pcode as pcode
from original_game_map_view import render_view
from verify_serialized_properties import inventory as property_inventory

ROOT = Path(__file__).resolve().parents[1]
STRUCTURE = ROOT / "tools/original_game_structure.json"
INVENTORY = ROOT / "docs/original-game-inventory.json"
CATALOG = ROOT / "docs/original-game-workflows.json"
REPORT = ROOT / "docs/original-game-map.md"
VIEW = ROOT / "docs/original-game-map.html"


def digest(data):
    return hashlib.sha256(data).hexdigest()


def hx(value):
    return f"0x{value:08X}"


def proc_id(start):
    return "procedure:" + hx(start)


def object_id(name):
    return "object:" + name


def control_id(form, name):
    return f"control:{form}.{name}"


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8"))


def write_json(path, value):
    path = Path(path)
    if path.name in (INVENTORY.name, STRUCTURE.name):
        # One machine record per line keeps large generated tables diffable.
        encode = lambda item: json.dumps(item, ensure_ascii=True, separators=(",", ":"))
        fields = []
        for key, item in value.items():
            body = "[\n" + ",\n".join("    " + encode(row) for row in item) + "\n  ]" if isinstance(item, list) else encode(item)
            fields.append("  " + encode(key) + ": " + body)
        rendered = "{\n" + ",\n".join(fields) + "\n}\n"
    else:
        rendered = json.dumps(value, ensure_ascii=True, indent=2) + "\n"
    path.write_text(rendered, encoding="utf-8")


def recover_menus(data, objects):
    """Read named class-19 records; 03 closes a menu nesting level.

    The child-list marker is persisted property 07 FF immediately before the
    property-list terminator. We do not infer hierarchy from menu names/captions.
    Enabled/visible/shortcut meanings of other property bytes remain unassigned.
    """
    owner = next(o for o in objects if o["name"] == "MDIForm1")
    records = []
    for control in owner["controls"]:
        name = control["name"]
        if not name.startswith("m"):
            continue
        marker = struct.pack("<H", len(name)) + name.encode("ascii") + b"\0"
        at = data.find(marker)
        if at < 5 or data.find(marker, at + 1) != -1:
            raise ParseError(f"menu record missing or ambiguous: {name}")
        head = at - 5
        size = struct.unpack_from("<I", data, head)[0]
        end = head + size
        body = at + len(marker)
        if not (0 <= head < body < end <= len(data)) or data[body] != 19:
            raise ParseError(f"invalid menu bounds/class: {name}")
        ordinal = data[head + 4]
        if ordinal != control["index"]:
            raise ParseError(f"menu ordinal disagrees with metadata: {name}")
        records.append({"id": "menu:" + name, "control": control_id(owner["name"], name),
            "offset": head, "size": size, "ordinal": ordinal,
            "sha256": digest(data[head:end]), "has_children": data[end-3:end] == b"\x07\xff\xff",
            "end": end})
    records.sort(key=lambda x: x["offset"])
    stack = []
    for index, record in enumerate(records):
        record["parent"] = stack[-1] if stack else None
        record["depth"] = len(stack)
        if record["has_children"]:
            stack.append(record["id"])
        gap_end = records[index+1]["offset"]-1 if index+1 < len(records) else record["end"]+2
        gap = data[record["end"]:gap_end]
        # The next record's 02 marker is excluded. Final 03 03 closes the
        # final menu and the containing designer; only consume menu closures.
        if any(byte != 3 for byte in gap):
            raise ParseError(f"unknown menu delimiter after {record['id']}")
        closures = len(gap) if index+1 < len(records) else min(len(stack), len(gap))
        if closures > len(stack):
            raise ParseError("menu stack underflow")
        for _ in range(closures):
            stack.pop()
        record.pop("end")
    if stack:
        raise ParseError("unclosed menu hierarchy")
    return records


def decode(pe, obj, descriptor):
    raw = pe.bytes_at(int(descriptor["start"], 16), descriptor["proc_size"])
    pos = 0
    instructions = []
    while pos < len(raw):
        # Descriptor-alignment tails are reported, not silently decoded as code.
        if not any(raw[pos:]) or (instructions and instructions[-1].mnemonic.startswith("ExitProc") and len(raw)-pos <= 3):
            break
        instruction, pos = pcode._decode_one(raw, pos)
        instructions.append(instruction)
    return raw, instructions, len(raw)-pos


def build(exe, structure):
    data = Path(exe).read_bytes()
    if digest(data) != pcode.KNOWN_SHA256 or structure["source_sha256"] != pcode.KNOWN_SHA256:
        raise ParseError("unsupported original executable SHA-256")
    metadata = inspect(Path(exe))
    pe = PeImage(data)
    pcode.OPS.update({key: tuple(value) for key, value in structure["opcode_schema"].items()})
    objects, controls, events, procedures, edges, references, dispatch = [], [], [], [], [], [], []
    by_info = {int(o["object_info"], 16): object_id(o["name"]) for o in metadata["objects"]}
    valid = [(o, p) for o in metadata["objects"] for p in o.get("procedures", [])
        if p.get("start") and p.get("object_info") == o.get("object_info")]
    by_code = {int(p["code_info"], 16): proc_id(int(p["start"], 16)) for o, p in valid}
    by_start = {int(p["start"], 16) for o, p in valid}
    for obj in metadata["objects"]:
        oid = object_id(obj["name"])
        objects.append({"id": oid, "name": obj["name"], "ordinal": obj["ordinal"], "kind": obj["kind"],
            "object_info": obj["object_info"], "constant_pool": obj["constant_pool"],
            "status": "inventoried"})
        if obj.get("controls_error"):
            raise ParseError("control inventory failed: " + obj["name"])
        for control in obj.get("controls", []):
            cid = control_id(obj["name"], control["name"])
            controls.append({"id": cid, "object": oid, "name": control["name"],
                "ordinal": control["ordinal"], "index": control["index"],
                "declared_event_slots": control["event_count"], "status": "inventoried"})
            for event in control["events"]:
                descriptor = event.get("procedure", {})
                if not descriptor.get("start") or int(descriptor["start"], 16) not in by_start:
                    raise ParseError("unresolved event descriptor: " + cid)
                events.append({"id": f"event:{obj['name']}.{control['name']}:{event['slot']}",
                    "control": cid, "slot": event["slot"], "procedure": proc_id(int(descriptor["start"], 16)),
                    "status": "bound-slot; event-name-not-inferred"})
    for obj, descriptor in valid:
        raw, instructions, tail = decode(pe, obj, descriptor)
        pid = proc_id(int(descriptor["start"], 16))
        pool = int(obj["constant_pool"], 16)
        for fact in structure.get("numeric_facts", []):
            if fact["procedure"] != pid:
                continue
            instruction = next((i for i in instructions if i.offset == fact["at"]), None)
            if instruction is None or instruction.mnemonic != fact["opcode"] or struct.unpack_from("<i", instruction.raw, len(instruction.raw)-4)[0] != fact["value"]:
                raise ParseError("numeric source fact changed: " + fact["id"])
        procedures.append({"id": pid, "object": object_id(obj["name"]), "start": descriptor["start"],
            "size": len(raw), "sha256": digest(raw), "instructions": len(instructions),
            "alignment_tail_bytes": tail, "status": "decoded; semantics-partial"})
        for instruction in instructions:
            index = pcode.pool_index(instruction)
            if index is not None:
                value = pe.u32(pool + index*4)
                references.append({"procedure": pid, "at": instruction.offset, "opcode": instruction.mnemonic,
                    "kind": "pool-address; role-unclassified", "value": hx(value)})
                if instruction.mnemonic.startswith("ImpAdCall"):
                    try:
                        if pe.bytes_at(value, 1) == b"\xba":
                            target = by_code.get(pe.u32(value+1))
                            if target:
                                edges.append({"source": pid, "target": target, "at": instruction.offset,
                                    "kind": "direct-pcode-thunk-call"})
                    except ParseError:
                        pass  # Runtime/import targets need not point into this PE.
            if instruction.mnemonic in ("NewIfNullAd", "NewIfNullPr"):
                index = struct.unpack_from("<H", instruction.raw, 1)[0]
                target = by_info.get(pe.u32(pool + index*4))
                if target:
                    edges.append({"source": pid, "target": target, "at": instruction.offset,
                        "kind": "object-reference; not-proof-of-show"})
            if instruction.mnemonic.startswith("FMem"):
                offset = 4 if instruction.key.startswith("FD") else 3
                if offset+2 <= len(instruction.raw):
                    member = struct.unpack_from("<H", instruction.raw, offset)[0]
                    references.append({"procedure": pid, "at": instruction.offset, "opcode": instruction.mnemonic,
                        "kind": "member-offset; receiver-unresolved", "value": f"0x{member:04X}"})
            if any(tag in instruction.mnemonic for tag in ("VCall", "Late", "Event")):
                dispatch.append({"procedure": pid, "at": instruction.offset, "opcode": instruction.mnemonic,
                    "status": "dynamic-target-unresolved"})
    # Check every serialized anchor. These are bounded known records, not an
    # assertion that every possible VB persistence encoding has been decoded.
    for section in ("layouts", "instances", "pictures"):
        for anchor in structure[section]:
            offset, size = anchor["offset"], anchor["size"]
            if offset < 0 or size <= 0 or offset+size > len(data) or digest(data[offset:offset+size]) != anchor["sha256"]:
                raise ParseError("serialized anchor changed: " + anchor["id"])
            rectangle = anchor.get("rectangle_evidence")
            if rectangle:
                actual = list(struct.unpack_from(rectangle["format"], data, rectangle["offset"]+1))
                field = "client_rect_twips" if section == "layouts" else "rect_twips"
                if actual != anchor[field]:
                    raise ParseError("rectangle disagrees with serialized bytes: " + anchor["id"])
            if section == "instances":
                head = offset+1
                encoded_size = struct.unpack_from("<I", data, head)[0]
                array = bool(encoded_size & 0x80000000)
                name_at = head + (7 if array else 5)
                length = struct.unpack_from("<H", data, name_at)[0]
                name_end = name_at+2+length
                name = data[name_at+2:name_end].decode("ascii")
                obj = anchor["object"].removeprefix("object:")
                actual_index = struct.unpack_from("<h", data, head+5)[0] if array else None
                classes = {0: "Pix", 1: "Label", 2: "Edit", 3: "Frame", 4: "Push", 5: "Check", 6: "Opt", 7: "Combo", 8: "LBox", 9: "HScroll", 10: "Scroll", 11: "Timer", 13: "Form", 24: "Img"}
                if (encoded_size & 0x7fffffff)+1 != size or data[name_end] != 0 or control_id(obj, name) != anchor["control"] or actual_index != anchor["array_index"] or classes.get(data[name_end+1], "unknown") != anchor["class"]:
                    raise ParseError("serialized instance binding disagrees with source: " + anchor["id"])
            if section == "pictures":
                payload = data[offset+8:offset+size]
                fmt = "BMP" if payload.startswith(b"BM") else "GIF" if payload.startswith(b"GIF8") else "ICO" if payload.startswith(b"\0\0\1\0") else "CUR" if payload.startswith(b"\0\0\2\0") else "unclassified"
                if fmt != anchor["format"]:
                    raise ParseError("picture format disagrees with source: " + anchor["id"])
    menus = recover_menus(data, metadata["objects"])
    property_report=property_inventory(Path(exe),structure)
    property_records={row['id']:row for row in property_report['records']}
    for row in structure['instances']:
        if row['property_parse']!=property_records[row['id']]['status']:
            raise ParseError('stale serialized property disposition: '+row['id'])
    recovered_pictures={(row['offset'],row['size'],row['sha256']) for row in property_report['pictures']}
    if recovered_pictures!={(row['offset'],row['size'],row['sha256']) for row in structure['pictures']}:
        raise ParseError('serialized picture property inventory is incomplete')
    timers = [{"id": "timer:" + row["id"].removeprefix("instance:"), "instance": row["id"],
        "object": row["object"], "control": row["control"], "serialized": row["timer_properties"],
        "bound_procedures": [e["procedure"] for e in events if e["control"] == row["control"]],
        "status": "serialized-values-and-bindings; enable-writers-and-delivery-partial"}
        for row in structure["instances"] if row["class"] == "Timer"]
    classes = {}
    for row in structure["instances"]:
        classes.setdefault(row["control"], set()).add(row["class"])
    menu_controls = {row["control"] for row in menus}
    for event in events:
        kinds = classes.get(event["control"], set())
        event["candidate_role"] = ("menu-action" if event["control"] in menu_controls else
            "timer-callback" if "Timer" in kinds else "button-event" if "Push" in kinds else
            "picture-or-label-event" if kinds.intersection({"Img", "Pix", "Label"}) else "lifecycle-or-control-event")
    return {"schema": 1, "source_sha256": digest(data), "source_bytes": len(data),
        "baseline_main": structure["baseline_main"], "objects": objects, "controls": controls,
        "events": events, "procedures": procedures, "menus": menus, "edges": edges,
        "timers": timers,
        "numeric_facts": structure.get("numeric_facts", []),
        "references": references, "unresolved_dispatch": dispatch,
        "layouts": structure["layouts"], "instances": structure["instances"], "pictures": structure["pictures"],
        "serialized_property_summary":dict(counts=property_report['counts'],opaque_marker_counts=property_report['opaque_marker_counts']),
        "limits": structure["limits"]}


def validate(inventory, catalog, root=ROOT):
    errors = []
    collections = ("objects", "controls", "events", "procedures", "menus", "layouts", "instances", "pictures", "timers")
    ids = [x["id"] for collection in collections for x in inventory[collection]]
    known = set(ids)
    if len(ids) != len(known):
        errors.append("duplicate stable ID")
    if inventory["source_sha256"] != catalog["source_sha256"] or inventory["source_sha256"] != pcode.KNOWN_SHA256:
        errors.append("catalog source hash mismatch")
    expected = catalog["expected_counts"]
    for key, count in expected.items():
        actual = sum(o["kind"] == "form_or_designer" for o in inventory["objects"]) if key == "forms" else len(inventory[key])
        if actual != count:
            errors.append(f"{key}: expected {count}, found {actual}")
    for collection in collections:
        for row in inventory[collection]:
            for key in ("object", "control", "procedure", "parent", "owner", "instance"):
                if row.get(key) and row[key] not in known:
                    errors.append(f"broken {key} link: {row['id']}")
    menu_by_id = {row["id"]: row for row in inventory["menus"]}
    for menu in inventory["menus"]:
        parent = menu_by_id.get(menu["parent"])
        if menu["depth"] != (parent["depth"]+1 if parent else 0):
            errors.append("invalid menu hierarchy depth: " + menu["id"])
    for picture in inventory["pictures"]:
        owner = next((row for key in ("instances", "objects") for row in inventory[key] if row["id"] == picture["owner"]), None)
        if owner and owner.get("object", owner["id"]) != picture["object"]:
            errors.append("picture owner belongs to another object: " + picture["id"])
    for edge in inventory["edges"]:
        if edge["source"] not in known or edge["target"] not in known:
            errors.append("broken graph edge")
    procedure_sizes = {p["id"]: p["size"] for p in inventory["procedures"]}
    for timer in inventory["timers"]:
        if any(p not in procedure_sizes for p in timer["bound_procedures"]):
            errors.append("invalid timer callback: " + timer["id"])
    for fact in inventory["numeric_facts"]:
        if fact["procedure"] not in procedure_sizes or not 0 <= fact["at"] < procedure_sizes[fact["procedure"]]:
            errors.append("invalid numeric fact source: " + fact["id"])
    for row in inventory["references"] + inventory["unresolved_dispatch"]:
        if row["procedure"] not in procedure_sizes or not 0 <= row["at"] < procedure_sizes[row["procedure"]]:
            errors.append("invalid instruction site")
    for edge in inventory["edges"]:
        if edge["source"] not in procedure_sizes or not 0 <= edge["at"] < procedure_sizes[edge["source"]]:
            errors.append("invalid edge site")
    workflow_ids = [w["id"] for w in catalog["workflows"]]
    if len(set(workflow_ids)) != len(workflow_ids):
        errors.append("duplicate workflow ID")
    covered = set()
    for workflow in catalog["workflows"]:
        covered.update(workflow["objects"])
        if not workflow["objects"] or any(o not in known for o in workflow["objects"]):
            errors.append("invalid workflow object: " + workflow["id"])
        if workflow["understanding"] not in ("partial", "unresolved"):
            errors.append("unsupported understanding claim: " + workflow["id"])
        if workflow["adaptation"] not in ("comparison-present", "partial", "missing", "intentional-difference", "unresolved"):
            errors.append("missing/invalid adaptation status: " + workflow["id"])
        if workflow["runtime"] != "pending-original-runtime":
            errors.append("runtime claim requires new evidence: " + workflow["id"])
        if not workflow["open_questions"] or not workflow["scenarios"]:
            errors.append("workflow lacks unresolved questions/scenarios: " + workflow["id"])
        for link in workflow["evidence"] + workflow["adaptation_files"] + workflow["tests"]:
            if not (root / link).is_file():
                errors.append(f"missing repository link: {workflow['id']} -> {link}")
        if any(p not in procedure_sizes for p in workflow.get("source_procedures", [])):
            errors.append("invalid workflow procedure: " + workflow["id"])
    missing = {o["id"] for o in inventory["objects"]} - covered
    if missing:
        errors.append("objects without workflow disposition: " + ", ".join(sorted(missing)))
    for row in catalog["crosscutting"]:
        if row["status"] not in ("partial", "unresolved", "intentional-difference"):
            errors.append("crosscutting claim requires evidence: " + row["id"])
        for path in row["evidence"] + row["adaptation_files"]:
            if not (root / path).is_file():
                errors.append("missing crosscutting link: " + path)
    menus = {menu["id"] for menu in inventory["menus"]}
    menu_ids = [row["id"] for row in catalog["menu_dispositions"]]
    if set(menu_ids) != menus or len(menu_ids) != len(menus):
        errors.append("menu dispositions do not cover exactly the original menu inventory")
    for row in catalog["menu_dispositions"]:
        if not row["disposition"] or any(w not in workflow_ids for w in row["workflows"]):
            errors.append("invalid menu disposition: " + row["id"])
    case_ids = [case["id"] for w in catalog["workflows"] for case in w["scenarios"]]
    if len(case_ids) != len(set(case_ids)):
        errors.append("duplicate runtime scenario ID")
    for table in catalog["catalog_families"]:
        if table["procedure"] not in procedure_sizes:
            errors.append("table initializer not in procedure inventory: " + table["id"])
        for path in table["adaptation_files"]:
            if not (root / path).is_file():
                errors.append("missing catalog importer: " + path)
    return errors


def render(inventory, catalog):
    count = lambda key: len(inventory[key])
    forms = sum(o["kind"] == "form_or_designer" for o in inventory["objects"])
    lines = ["# Original-game map", "", "This map starts from the supported original executable; OpenGDS2 is a separate comparison layer.",
        "Stable IDs, hashes and relationships are in [the inventory](original-game-inventory.json); authored workflows, runtime scenarios and gaps are in [the catalog](original-game-workflows.json). Open [the standalone searchable view](original-game-map.html) locally to browse workflows, menus, event bindings, procedures and graph edges without a server or external assets.",
        "", f"Original SHA-256: `{inventory['source_sha256']}`. Comparison baseline: `{inventory['baseline_main']}`.", "",
        "## Coverage means", "",
        "Every discovered item has an inventory entry, and every compiled object has a workflow disposition. This is **100% accounting of the enumerated sets**, not 100% game understanding or adaptation parity. The official archive is now inventoried and imported resources are checked separately. One MDI root encoding, opaque property meanings, dynamically dispatched targets and native runtime behavior remain open boundaries.", "",
        "| Set | Accounted | Meaning |", "| --- | ---: | --- |",
        f"| Compiled objects | {count('objects')} / {count('objects')} | {forms} forms/designers, two modules |",
        f"| Metadata control definitions | {count('controls')} / {count('controls')} | Includes form and menu definitions; arrays are definitions |",
        f"| Bound event slots | {count('events')} / {count('events')} | Slot-to-procedure bindings; names not guessed |",
        f"| Valid procedure descriptors | {count('procedures')} / {count('procedures')} | {sum(p['instructions'] for p in inventory['procedures']):,} decoded instructions; semantics remain partial |",
        f"| Known serialized control instances | {count('instances')} / {count('instances')} | Array instances and decorative controls; excludes menus and form roots |",
        f"| Serialized menus | {count('menus')} / {count('menus')} | Parents recovered from persisted child markers and closing delimiters |",
        f"| Known picture properties | {count('pictures')} / {count('pictures')} | Includes repeated images and icons; not the full archive |",
        f"| Known serialized timers | {count('timers')} / {count('timers')} | Stored intervals/enabled values and event bindings; dynamic writers remain partial |",
        f"| Original runtime workflow acceptance | 0 / {len(catalog['workflows'])} | Original was not executed in this environment |", "",
        "Decoded instruction coverage does not prove call-graph completeness. Pool addresses may denote imports, globals, objects or constants. Member offsets retain unknown receiver identity. Object references do not prove a window opens or is modal.", "",
        f"Recorded graph: **{count('edges'):,}** direct-call/object-reference edges; **{count('references'):,}** operand-reference sites; **{count('unresolved_dispatch'):,}** dynamic-dispatch candidate sites remain unresolved.", "",
        "## Original menus", "", "Names below are original technical identifiers. Indentation comes from serialization, not English translation or the current port menu. Shortcut/enabled/visible semantics need a further property pass.", "",
        "Each entry states what is available in OpenGDS2 and where the player can find it. These labels describe access and implementation found in the code; original-game behavior and runtime parity remain unverified.", "",
        "- **Available:** the action exists at the stated menu or button.",
        "- **Available elsewhere:** the feature exists through the stated alternative path; the original direct menu item is missing.",
        "- **Missing menu access:** related controls exist at the stated location, but the original in-game menu entry is missing. Any uncertainty about equivalent behavior is stated separately.",
        "- **Missing option:** no equivalent player-selectable choice was found. Existing graphics or number rendering may still work without that choice.",
        "- **Missing functionality:** the gameplay action/transaction is not implemented; there is no alternate player path.",
        "- **Unresolved:** the evidence does not yet establish an equivalent action or its behavior; this is not counted as confirmed missing functionality.",
        "- **Menu organization / Separator:** describes a container or divider, not a gameplay feature.", ""]
    dispositions = {row["id"]: row for row in catalog["menu_dispositions"]}
    for menu in inventory["menus"]:
        row = dispositions[menu["id"]]
        lines.append("  " * menu["depth"] + "- " + row["label"] + " — `" + menu["id"] + "`: " + row["disposition"])
    lines += ["", "## Workflow and adaptation index", "", "All rows retain partial understanding and pending original runtime acceptance. Comparison files and automated tests establish where to investigate; they do not certify native parity.", "", "| Stable ID | Original objects | Adaptation disposition | Evidence |", "| --- | --- | --- | --- |"]
    for w in catalog["workflows"]:
        objects = ", ".join("`" + o.removeprefix("object:") + "`" for o in w["objects"])
        links = " · ".join(f"[{Path(p).stem}]({Path(p).name})" for p in w["evidence"])
        lines.append(f"| {w['id']}: {w['title']} | {objects} | {w['adaptation']} | {links} |")
    lines += ["", "## Original timer inventory", "", "Values are serialized requests in milliseconds, not observed delivery cadence. `unknown/default` means no explicit Enabled value was recovered; it does not mean disabled. An unbound timer is retained rather than assigned an invented function. Runtime writes can change these values.", "", "| Original control | Interval | Explicit Enabled | Bound procedures |", "| --- | ---: | --- | --- |"]
    for timer in inventory["timers"]:
        enabled = timer["serialized"]["Enabled"]
        handlers = ", ".join("`" + p + "`" for p in timer["bound_procedures"]) or "none bound"
        lines.append(f"| `{timer['control'].removeprefix('control:')}` | {timer['serialized']['Interval']} | {'unknown/default' if enabled is None else enabled} | {handlers} |")
    lines += ["", "## Crosscutting map and next evidence", ""]
    for row in catalog["crosscutting"]:
        lines += [f"### {row['id']}: {row['title']}", "", row["finding"], "", "Next: " + row["next_evidence"], ""]
    lines += ["## Catalog and audio source anchors", "", "These are identified families and their current comparison/import paths. Their row counts are recovered subsets, not a denominator for every original table.", "", "| ID | Family | Source initializer | Comparison paths |", "| --- | --- | --- | --- |"]
    for table in catalog["catalog_families"]:
        links = " · ".join(f"[{Path(p).name}](../{p})" for p in table["adaptation_files"])
        lines.append(f"| {table['id']} | {table['family']} | `{table['procedure']}` | {links} |")
    lines += ["", "Audio selector/trigger anchors: [original_audio_evidence.json](../tools/original_audio_evidence.json). Native ordered save records: [native_save_evidence.json](../tools/native_save_evidence.json). Both retain narrower claims than complete audio or persistence mapping.", ""]
    lines += ["## Prioritized gap queue", "", "These are evidence tasks or known adaptation differences, not authorization to invent or implement new gameplay.", "", "| Priority | ID | Work remaining |", "| --- | --- | --- |"]
    for gap in catalog["gaps"]:
        lines.append(f"| {gap['priority']} | {gap['id']} | {gap['task']} |")
    lines += ["", "## Reproduce and query", "", "With the supported original executable kept outside the checkout:", "", "```sh", "python3 tools/map_original_game.py --exe /private/path/GameDev2DX.exe --check", "python3 -m unittest discover -s tests -p 'test_original_game_map.py'", "python3 tools/map_original_game.py --query fGMDevW1", "```", "",
        "`--check` rebuilds metadata, menu hierarchy, procedure decoding and reference graph; rechecks every known serialized boundary/hash; and compares the deterministic inventory/report with the committed files. No target code executes. `--write` deliberately refreshes generated files after reviewed structure/catalog edits. Without `--exe`, checks validate catalog accounting and links only. `--query` returns one object's controls, bindings, procedures, resources, callers and workflow IDs.", "",
        "The structural anchors are build-specific recovered boundaries. Hash verification does not improve a partial property parse. Current known limits:", ""]
    lines += ["- " + limit for limit in inventory["limits"]]
    lines += ["", "## Runtime acceptance protocol", "", "Execute the catalog's scenarios against a disposable copy of the original, recording executable/archive hashes, scenario ID, starting save hash, actions, observed ordering, captures and state deltas. Pair each recording with the adaptation on an equivalent setup; record differences explicitly. Test normal, cancel, unavailable/failure, repeated input, automatic interruption, save/load and timing paths. A scenario is only accepted after its evidence is reviewed; static fingerprints and a modern smoke test cannot accept it.", "",
        "Original executables, archives, extracted strings, images/audio, saves, captures and full traces stay private. Public records contain only authored descriptions and technical metadata. Keep presentation issue #95 open.", ""]
    return "\n".join(lines)


def query(inventory, catalog, term):
    matches = [o for o in inventory["objects"] if term.lower() in o["name"].lower()]
    objects = {o["id"] for o in matches}
    value = None
    try:
        value = hx(int(term.removeprefix("procedure:"), 16))
    except ValueError:
        pass
    workflows = [w for w in catalog["workflows"] if w["id"].lower() == term.lower() or objects.intersection(w["objects"])]
    objects.update(o for w in workflows for o in w["objects"])
    procedures = {p["id"] for p in inventory["procedures"] if p["object"] in objects or p["start"] == value}
    references = [r for r in inventory["references"] if r["procedure"] in procedures or r["value"] == value]
    return {"objects": matches,
        "workflows": workflows,
        "controls": [c for c in inventory["controls"] if c["object"] in objects],
        "events": [e for e in inventory["events"] if e["procedure"] in procedures],
        "procedures": [p for p in inventory["procedures"] if p["id"] in procedures],
        "edges": [e for e in inventory["edges"] if e["source"] in procedures or e["target"] in objects | procedures],
        "references": references,
        "pictures": [p for p in inventory["pictures"] if p["object"] in objects]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--exe", type=Path)
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", action="store_true")
    mode.add_argument("--write", action="store_true")
    parser.add_argument("--query")
    args = parser.parse_args()
    try:
        catalog = read_json(CATALOG)
        inventory = build(args.exe, read_json(STRUCTURE)) if args.exe else read_json(INVENTORY)
        errors = validate(inventory, catalog)
        if errors:
            raise ValueError("\n".join(errors))
        report = render(inventory, catalog)
        if args.write:
            if not args.exe:
                raise ValueError("--write requires --exe; never refresh from unverified cached inventory")
            write_json(INVENTORY, inventory)
            REPORT.write_text(report, encoding="utf-8")
            VIEW.write_text(render_view(inventory, catalog), encoding="utf-8")
        if args.check:
            if args.exe and inventory != read_json(INVENTORY):
                raise ValueError("rebuilt original inventory differs from committed inventory")
            if report != REPORT.read_text(encoding="utf-8"):
                raise ValueError("generated map report is stale")
            if render_view(inventory, catalog) != VIEW.read_text(encoding="utf-8"):
                raise ValueError("generated map view is stale")
        if args.query:
            print(json.dumps(query(inventory, catalog, args.query), indent=2))
        else:
            scope = "fresh original rebuild" if args.exe else "cached inventory/catalog checks"
            print(f"PASS ({scope}): {len(inventory['objects'])} objects, {len(inventory['procedures'])} procedures, {len(catalog['workflows'])} workflow dispositions; original runtime pending")
        return 0
    except (OSError, ValueError, ParseError, KeyError, struct.error) as error:
        print("map validation failed: " + str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
