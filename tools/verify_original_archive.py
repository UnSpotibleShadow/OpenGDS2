#!/usr/bin/env python3
"""Verify the supported, privately supplied LZH; publish metadata only, never assets."""
import argparse
import hashlib
import json
import struct
from pathlib import Path, PurePosixPath
from extract_lzh import entries, lh5_decompress

ARCHIVE_SHA256 = '118fc868e3b888ec86b9dcd958f52109d0074717e8e78d9fbc44bd03ecdfc0b4'
SOURCE_URL = 'https://kairosoft.net/File/GameDev2DX.lzh'


def crc16(data):
    value = 0
    for byte in data:
        value ^= byte
        for _ in range(8):
            value = (value >> 1) ^ (0xA001 if value & 1 else 0)
    return value


def inventory(path):
    data = path.read_bytes()
    if hashlib.sha256(data).hexdigest() != ARCHIVE_SHA256:
        raise ValueError('Unsupported or damaged original archive')
    rows, directories, names, position = [], [], set(), 0
    for raw, name, method, packed, size, offset in entries(path):
        relative = PurePosixPath(name)
        if relative.is_absolute() or '..' in relative.parts or name in names:
            raise ValueError('Unsafe or duplicate archive member')
        names.add(name)
        expected_crc = struct.unpack_from('<H', raw, position + 21)[0]
        if method == b'-lhd-':
            directories.append(name)
        else:
            payload = raw[offset:offset + packed]
            decoded = lh5_decompress(payload, size) if method == b'-lh5-' else payload if method == b'-lh0-' else None
            if decoded is None or len(decoded) != size or crc16(decoded) != expected_crc:
                raise ValueError('Unsupported or damaged archive member: ' + name)
            rows.append(dict(path=name, method=method.decode('ascii'), bytes=size,
                             crc16=expected_crc, sha256=hashlib.sha256(decoded).hexdigest()))
        position = offset + packed
    if any(data[position:]):
        raise ValueError('Unaccounted archive trailer')
    return dict(source_url=SOURCE_URL, archive_sha256=ARCHIVE_SHA256, archive_bytes=len(data),
                directories=directories, files=rows, crc_verified=len(rows),
                payload_end=position, trailing_zero_bytes=len(data) - position)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive', type=Path)
    parser.add_argument('--write', type=Path)
    parser.add_argument('--check', type=Path)
    args = parser.parse_args()
    result = inventory(args.archive)
    if args.check and result != json.loads(args.check.read_text()):
        raise ValueError('Archive metadata differs from the recorded inventory')
    if args.write:
        args.write.write_text(json.dumps(result, indent=2) + '\n')
    print(f"PASS original archive: {len(result['directories'])} directories, {len(result['files'])} files, all sizes and CRC16s verified")


if __name__ == '__main__':
    main()
