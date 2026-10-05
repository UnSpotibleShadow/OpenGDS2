# Bounded development execution and recovery

The interrupted M54 main-ref update completed remotely. Read-back on recovery
confirmed `main` at `aa9787a1d614a40a11b064417de3090d9b3b7da3`, the expected
checkpoint commit; repeating the ref write was unnecessary. Its M54 game changes
remain merged through [PR #48](https://github.com/UnSpotibleShadow/OpenGDS2/pull/48).

`AGENTS.md` now makes bounded execution and read-before-retry recovery part of
the repository's development instructions. These are development tools; game
rules, original assets and the required LZH import are unchanged.

## Local process watchdog

From the repository, run an individual check with an explicit deadline:

```sh
python3 tools/bounded_process.py --timeout 180 --heartbeat 15 \
  --label godot-core --checkpoint /absolute/workspace/temp/godot-core.json \
  --log /absolute/workspace/temp/godot-core.log \
  --fail-pattern 'SCRIPT ERROR:|Unhandled exception' -- \
  /absolute/path/to/godot --headless --path /absolute/project \
  --script res://tests/core_loop_smoke.gd
```

Use the project's actual Godot/C# build environment and isolated user-data
directories for smoke tests. The watchdog does not install a toolchain or bypass
SDK/export restrictions. Log/checkpoint paths should stay outside the repository.
`--quiet` keeps child output in the log while retaining heartbeat/status output.

The wall deadline does not reset when a process emits repeated output. Optional
`--idle-timeout` detects a normally chatty process becoming silent; it defaults to
disabled to avoid treating legitimate quiet work as a hang. Exit codes are 124
for wall timeout, 125 for silence timeout, 70 for fatal-output match and 130 for
interruption. Ordinary child exit codes are preserved. Atomic checkpoints record
status and elapsed time without command arguments or environment variables.

On POSIX systems the command gets its own process group. Timeout/interruption
terminates that group, including a descendant holding stdout open after its
parent has exited. Windows termination covers the immediate process; equivalent
descendant cleanup via a Windows Job Object is not implemented. Independent
processes which deliberately leave the group are outside this cleanup boundary.

## Connector deadline and ambiguous-write guard

`tools/connector_guard.mjs` exports `createConnectorGuard`. Supply a durable
checkpoint callback and a read-back verifier for mutations:

```js
const guard = createConnectorGuard({
  timeoutMs: 20000,
  checkpoint: saveRecoveryJson,
  pendingWrite: recoveredUnknownWrite // null for a new, reconciled operation
});
const result = await guard.run({
  label: "Update main reference",
  mutation: true,
  expected: {before: oldSha, after: targetSha},
  call: () => updateRef({sha: targetSha, force: false}),
  verify: async () => (await readMainRef()) === targetSha
});
```

The caller must first verify the current ref and persist its expected old/new
state. `verified` means the read-back confirmed the intended effect. A rejected,
failed, timed-out or unverified mutation returns `unknown` and blocks subsequent
mutations. Late completion of the original promise cannot silently clear that
state. `guard.reconcile(readBackVerifier)` clears it only on confirmed success.
Restore `pendingWrite` from the journal after restarting the orchestrator. A
different SHA can be a newer concurrent update and must not trigger a force push.

Read calls have bounded waits too. There are no automatic retries. Checkpoint
writes themselves have deadlines; a mutation cannot start if its starting
checkpoint is unavailable. Logs/checkpoints do not include connector errors,
responses, credentials or private source data unless a caller explicitly supplies
those as metadata; callers should supply only labels and necessary public IDs.

This wrapper bounds each call and verification phase separately. It cannot
cancel a server-side write or repair a ChatGPT UI/connector transport stall.
Local Node tests exercise the timer behavior; code-mode use also needs short
yield intervals and human-readable progress. Do not describe a timeout as proof
that a remote operation failed.

## Regression checks

```sh
python3 -m unittest discover -s tests -p test_bounded_process.py
node tests/connector_guard_test.mjs
```

Six Python cases cover successful/nonzero exits, quiet hangs, output-producing
hangs, optional silence deadlines, fatal script output, POSIX descendant cleanup
and invalid timeout inputs. Eight JavaScript cases cover verified writes,
ambiguous writes, blocked replay, late completion, read/verification timeouts,
checkpoint failures, restored/concurrent operations and invalid configuration.
The full Python suite also includes the watchdog cases. GitHub Actions remain
paused; all checks run locally.
