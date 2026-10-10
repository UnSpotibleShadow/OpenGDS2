#!/usr/bin/env python3
"""Run the normal SDK/Godot tools on this restricted Linux build host.

This does not compile game code itself or modify exported native runtimes.
Use bounded_process.py around each invocation; no watchdog is hidden here.
"""
import argparse
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
WORKSPACE = ROOT.parent

def prepare_environment(state, sdk, godot):
    state = state.resolve()
    state.mkdir(parents=True, exist_ok=True)
    temp = state / 'tmp'
    (temp / '.dotnet/shm').mkdir(parents=True, exist_ok=True)
    for path in [state / 'cli-home/.dotnet', state / 'nuget', state / 'godot-data']:
        path.mkdir(parents=True, exist_ok=True)
    # SDK startup queries Process.StartTime. Do not emulate arbitrary process
    # command lines, addresses or memory maps; unsupported queries still fail.
    (state / 'process.stat').write_text('1 (dotnet) R ' + ' '.join(['0'] * 49) + '\n')
    shim = state / 'sdk-paths.so'
    thread = state / 'thread-entry.so'
    for source, output in [(ROOT / 'tools/workspace_sdk_paths.c', shim),
                           (ROOT / 'tools/workspace_thread_entry.c', thread)]:
        subprocess.run(['cc', '-shared', '-fPIC', '-O2', '-o', str(output), str(source), '-ldl', '-pthread'], check=True)
    env = os.environ.copy()
    env.update(LD_PRELOAD=str(thread) + ':' + str(shim),
               OPENGDS2_SDK_STATE=str(state), DOTNET_ROOT=str(sdk),
               DOTNET_EnableDiagnostics='0', DOTNET_PROCESSOR_COUNT='2',
               DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_CLI_HOME=str(state / 'cli-home'),
               NUGET_PACKAGES=str(state / 'nuget'), TMPDIR=str(temp),
               XDG_DATA_HOME=str(state / 'godot-data'))
    env['PATH'] = str(sdk) + os.pathsep + str(godot.parent) + os.pathsep + env.get('PATH', '')
    # Godot embeds MSBuild for project inspection. Without /proc/self/exe its
    # host discovery has no executable path; identify the installed SDK explicitly.
    sdks = sorted((sdk / 'sdk').glob('*/MSBuild.dll'), key=lambda p: tuple(int(x) for x in p.parent.name.split('.')))
    if not sdks:
        raise FileNotFoundError('The supplied SDK contains no MSBuild.dll')
    env['MSBUILD_EXE_PATH'] = str(sdks[-1])
    return env

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--state', type=Path, default=WORKSPACE / 'toolchain/sdk-state')
    parser.add_argument('--sdk', type=Path, default=WORKSPACE / 'toolchain/dotnet')
    parser.add_argument('--godot', type=Path, default=WORKSPACE / 'toolchain/godot/Godot_v4.4.1-stable_mono_linux_x86_64/Godot_v4.4.1-stable_mono_linux.x86_64')
    parser.add_argument('command', nargs=argparse.REMAINDER)
    args = parser.parse_args()
    command = args.command[1:] if args.command[:1] == ['--'] else args.command
    if not command:
        parser.error('Supply dotnet or Godot and its arguments after --')
    env = prepare_environment(args.state, args.sdk.resolve(), args.godot.resolve())
    return subprocess.run(command, env=env).returncode

if __name__ == '__main__':
    raise SystemExit(main())
