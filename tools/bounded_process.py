#!/usr/bin/env python3
"""Run a local check with a wall deadline, optional silence deadline and checkpoint.

POSIX children share an isolated process group so a timeout stops their descendants.
Checkpoints record a label/status, never command arguments or environment variables.
"""
import argparse
import json
import math
import os
from pathlib import Path
import queue
import re
import signal
import subprocess
import sys
import threading
import time


def positive(value):
    value = float(value)
    if not math.isfinite(value) or value <= 0:
        raise argparse.ArgumentTypeError("must be finite and positive")
    return value


def nonnegative(value):
    value = float(value)
    if not math.isfinite(value) or value < 0:
        raise argparse.ArgumentTypeError("must be finite and nonnegative")
    return value


def checkpoint(path, label, status, elapsed, **extra):
    if path is None:
        return
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_name(path.name + ".tmp")
    temporary.write_text(json.dumps({"label": label, "status": status,
        "elapsed_seconds": round(elapsed, 3), **extra}, indent=2) + "\n")
    os.replace(temporary, path)


def stop(process):
    # The parent may have exited while a child still holds stdout open.
    def send(sig):
        try:
            if os.name == "posix":
                os.killpg(process.pid, sig)
            elif process.poll() is None:
                process.terminate() if sig == signal.SIGTERM else process.kill()
        except ProcessLookupError:
            pass
    send(signal.SIGTERM)
    try:
        process.wait(timeout=0.5)
    except subprocess.TimeoutExpired:
        pass
    # Always kill the group: descendants can outlive a successfully reaped parent.
    send(signal.SIGKILL if os.name == "posix" else signal.SIGTERM)
    try:
        process.wait(timeout=0.5)
    except subprocess.TimeoutExpired:
        pass


def run(args):
    start = time.monotonic()
    checkpoint(args.checkpoint, args.label, "starting", 0)
    pattern = re.compile(args.fail_pattern) if args.fail_pattern else None
    process = None
    log = open(args.log, "wb") if args.log else None
    interrupted = False
    def interrupt(_signum, _frame):
        nonlocal interrupted
        interrupted = True
    previous = signal.signal(signal.SIGTERM, interrupt)
    try:
        process = subprocess.Popen(args.command, stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT, start_new_session=os.name == "posix")
        checkpoint(args.checkpoint, args.label, "running", 0, pid=process.pid)
        chunks = queue.Queue(maxsize=256)
        def collect():
            try:
                for line in iter(process.stdout.readline, b""):
                    chunks.put(line)
            finally:
                chunks.put(None)
        reader = threading.Thread(target=collect, daemon=True)
        reader.start()
        last_output = start
        next_heartbeat = start + args.heartbeat
        eof = False
        while True:
            now = time.monotonic()
            elapsed = now - start
            status = None
            if interrupted:
                status, code = "interrupted", 130
            elif elapsed >= args.timeout:
                status, code = "timed_out", 124
            elif args.idle_timeout and now - last_output >= args.idle_timeout:
                status, code = "output_stalled", 125
            if status:
                stop(process)
                checkpoint(args.checkpoint, args.label, status, elapsed, exit_code=code)
                print(f"[watchdog] {args.label}: {status} after {elapsed:.1f}s", file=sys.stderr, flush=True)
                return code
            if now >= next_heartbeat:
                print(f"[watchdog] {args.label}: running {elapsed:.1f}s; deadline {args.timeout:g}s", file=sys.stderr, flush=True)
                checkpoint(args.checkpoint, args.label, "running", elapsed, pid=process.pid)
                next_heartbeat = now + args.heartbeat
            try:
                line = chunks.get(timeout=min(0.1, max(0.001, args.timeout-elapsed)))
            except queue.Empty:
                line = b""
            if line is None:
                eof = True
            elif line:
                last_output = time.monotonic()
                if log:
                    log.write(line)
                    log.flush()
                if not args.quiet:
                    sys.stdout.buffer.write(line)
                    sys.stdout.buffer.flush()
                if pattern and pattern.search(line.decode("utf-8", errors="replace")):
                    stop(process)
                    checkpoint(args.checkpoint, args.label, "fatal_output", time.monotonic()-start, exit_code=70)
                    return 70
            if eof and process.poll() is not None:
                code = process.returncode
                checkpoint(args.checkpoint, args.label, "completed" if code == 0 else "failed",
                    time.monotonic()-start, exit_code=code)
                return code if code >= 0 else 128-code
    except KeyboardInterrupt:
        if process:
            stop(process)
        checkpoint(args.checkpoint, args.label, "interrupted", time.monotonic()-start, exit_code=130)
        return 130
    except Exception:
        if process:
            stop(process)
        checkpoint(args.checkpoint, args.label, "failed", time.monotonic()-start, exit_code=1)
        raise
    finally:
        signal.signal(signal.SIGTERM, previous)
        if log:
            log.close()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--timeout", type=positive, default=180)
    parser.add_argument("--idle-timeout", type=nonnegative, default=0,
        help="optional deadline since last output; zero disables it")
    parser.add_argument("--heartbeat", type=positive, default=15)
    parser.add_argument("--label", default="local check")
    parser.add_argument("--checkpoint", help="JSON recovery state outside the repository")
    parser.add_argument("--log", help="captured output file; parent directory must exist")
    parser.add_argument("--quiet", action="store_true", help="keep child output in the log only")
    parser.add_argument("--fail-pattern", help="stop immediately on matching output, e.g. SCRIPT ERROR")
    parser.add_argument("command", nargs=argparse.REMAINDER)
    args = parser.parse_args()
    if args.command and args.command[0] == "--":
        args.command.pop(0)
    if not args.command:
        parser.error("a command is required after --")
    return run(args)


if __name__ == "__main__":
    sys.exit(main())
