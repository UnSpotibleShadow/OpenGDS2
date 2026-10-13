#!/usr/bin/env python3
"""Make a local, linked reference of imported artwork; never embed/copy pixels."""
import argparse
from html import escape
from pathlib import Path


def family(relative):
    name = relative.name.lower()
    if 'recovered' in [part.lower() for part in relative.parts]:
        return 'Serialized executable resources (offset inventory)'
    if name.startswith('office'):
        return 'Office base, sprite atlas and masks'
    if name.startswith('hardware'):
        return 'Target device pictures (separate embedded images)'
    if 'human' in [part.lower() for part in relative.parts]:
        return 'Staff portraits (separate archive images)'
    if 'embedded' in [part.lower() for part in relative.parts]:
        return 'Embedded illustrations and controls'
    return 'Other archive / locally imported artwork'


def build(cache):
    groups = {}
    for path in sorted(cache.rglob('*')):
        if path.is_file() and path.suffix.lower() in {'.bmp', '.png', '.gif'}:
            relative = path.relative_to(cache)
            groups.setdefault(family(relative), []).append((path, relative))
    if not groups:
        raise ValueError('No imported artwork found in that cache.')
    rows = ['<!doctype html><meta charset="utf-8"><title>Imported artwork reference</title>',
        '<style>body{font:16px sans-serif;margin:24px;background:#eee}section{display:flex;flex-wrap:wrap;gap:16px}',
        'figure{margin:0;padding:12px;background:white;max-width:650px}img{max-width:620px;max-height:330px;image-rendering:pixelated}',
        'figcaption{margin-top:8px;overflow-wrap:anywhere}</style>',
        '<h1>Imported artwork reference</h1><p>The office sprite atlas is one resource. Device pictures, staff portraits and illustrations are separate.</p>',
        '<p>This index covers images present in this local cache, not every serialized resource in the original executable. Click a preview for the original-size file.</p>']
    for label, files in groups.items():
        rows.append(f'<h2>{escape(label)} ({len(files)})</h2><section>')
        for path, relative in files:
            url = escape(path.resolve().as_uri(), quote=True)
            title = escape(relative.as_posix())
            rows.append(f'<figure><a href="{url}"><img src="{url}" alt="{title}" loading="lazy"></a><figcaption>{title}</figcaption></figure>')
        rows.append('</section>')
    return '\n'.join(rows)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--cache', type=Path, required=True, help='Your imported original cache directory')
    parser.add_argument('--output', type=Path, required=True, help='Private local HTML file, outside the source checkout')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    if args.output.resolve().is_relative_to(root):
        parser.error('Keep the generated reference outside the source checkout.')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(build(args.cache.resolve()), encoding='utf-8')
    print(args.output.resolve())


if __name__ == '__main__':
    main()
