# VB6 P-Code research notes

OpenGDS2's reverse-engineering tools are static and do not execute `GameDev2DX.exe`.

## Why P-Code matters

The VB project metadata has `ProjectInfo.aNativeCode == 0`, identifying this executable as a P-Code
build. Its form event tables point to `CodeInfo`/procedure descriptors whose procedure size gives the
P-Code start address.

For a procedure with `CodeInfo = C` and `ProcSize = S`:

`PCodeStart = C - S`

This relationship is reproduced by `tools/inspect_vb6.py` and independently matches the startup
procedures discovered manually.

## Current decoder scope

`tools/scan_startup_pcode.py` is intentionally tiny. It only recognizes the sequence used to prove
the indexed startup choice:

- `0x7F` — `ILdI2`
- `0xF4` — `LitI2_Byte`
- `0xC6` — `EqI2`
- `0x1C` — `BranchF`

The opcode identities and sizes come from the public Semi-VB-Decompiler opcode table. A future tool
can grow this into a full disassembler/decompiler, but M3 avoids pretending that partial opcode
coverage is complete.

## Reproducible commands

```bash
python tools/inspect_vb6.py /path/to/GameDev2DX.exe --out generated/vb6
cd tools
python scan_startup_pcode.py /path/to/GameDev2DX.exe --out ../generated/vb6/pcode_startup_evidence.txt
```

## M4 target procedures

The next useful decoding targets are no longer guessed from form names. Static metadata plus the
serialized designer blocks identify:

- `fGMSKoyou.Command1` (Hire): P-Code start `0x0099FFC8`, 1480 bytes.
- `fGMSKoyou` form event slot 6: P-Code start `0x009BABFC`, 3508 bytes.
- `fGMSKoyouKz.Command1` (renewal decision): P-Code start `0x00995CDC`, 884 bytes.
- `fGMSKoyouKz` form event slot 6: P-Code start `0x009BF5A0`, 3940 bytes.

The M4 form-string scanner provides the UI/control semantics needed to interpret those procedures as
the opcode decoder grows.
