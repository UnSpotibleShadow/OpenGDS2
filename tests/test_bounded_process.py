"""Watchdog tests use authored child processes; no Godot or original data needed."""
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import time
import unittest

RUNNER = Path(__file__).resolve().parents[1] / "tools/bounded_process.py"


class BoundedProcessTests(unittest.TestCase):
    def invoke(self, script, *options):
        with tempfile.TemporaryDirectory() as directory:
            journal = Path(directory) / "checkpoint.json"
            log = Path(directory) / "output.log"
            result = subprocess.run([sys.executable, str(RUNNER), "--label", "authored fixture",
                "--checkpoint", str(journal), "--log", str(log), "--quiet", *options,
                "--", sys.executable, "-c", script], capture_output=True, timeout=6)
            return result, json.loads(journal.read_text()), log.read_text()

    def test_success_and_nonzero_exit_are_recorded_without_command_arguments(self):
        result, state, log = self.invoke("print('fixture output')", "--timeout", "2")
        self.assertEqual(result.returncode, 0)
        self.assertEqual(state["status"], "completed")
        self.assertIn("fixture output", log)
        self.assertNotIn("command", state)
        result, state, _ = self.invoke("raise SystemExit(7)", "--timeout", "2")
        self.assertEqual(result.returncode, 7)
        self.assertEqual(state["status"], "failed")

    def test_silent_hang_has_an_absolute_deadline(self):
        start = time.monotonic()
        result, state, _ = self.invoke("import time; time.sleep(20)", "--timeout", ".3", "--heartbeat", ".1")
        self.assertEqual(result.returncode, 124)
        self.assertEqual(state["status"], "timed_out")
        self.assertLess(time.monotonic()-start, 3)
        self.assertIn(b"running", result.stderr)

    def test_repeated_output_cannot_extend_the_absolute_deadline(self):
        script = "import time\nwhile True:\n print('busy',flush=True)\n time.sleep(.02)"
        result, state, log = self.invoke(script, "--timeout", ".3", "--idle-timeout", ".2")
        self.assertEqual(result.returncode, 124)
        self.assertEqual(state["status"], "timed_out")
        self.assertIn("busy", log)

    def test_optional_silence_deadline_and_fatal_output_stop_early(self):
        result, state, _ = self.invoke("import time; time.sleep(20)", "--timeout", "3", "--idle-timeout", ".2")
        self.assertEqual(result.returncode, 125)
        self.assertEqual(state["status"], "output_stalled")
        result, state, log = self.invoke("import time; print('SCRIPT ERROR: authored fixture',flush=True); time.sleep(20)",
            "--timeout", "3", "--fail-pattern", "SCRIPT ERROR:")
        self.assertEqual(result.returncode, 70)
        self.assertEqual(state["status"], "fatal_output")
        self.assertIn("SCRIPT ERROR:", log)

    @unittest.skipUnless(os.name == "posix", "Process group cleanup is a POSIX safeguard")
    def test_parent_exit_does_not_leave_a_hung_stdout_holding_descendant(self):
        with tempfile.TemporaryDirectory() as directory:
            marker = Path(directory) / "unexpected-child-output"
            child = f"import time,pathlib;time.sleep(1);pathlib.Path({str(marker)!r}).write_text('alive')"
            parent = f"import subprocess,sys;subprocess.Popen([sys.executable,'-c',{child!r}])"
            result, state, _ = self.invoke(parent, "--timeout", ".3")
            self.assertEqual(result.returncode, 124)
            self.assertEqual(state["status"], "timed_out")
            time.sleep(1.1)
            self.assertFalse(marker.exists(), "Timed-out descendant continued after its parent exited")

    def test_invalid_deadlines_do_not_start_a_child(self):
        for value in ["0", "-1", "nan", "inf"]:
            result = subprocess.run([sys.executable, str(RUNNER), "--timeout", value,
                "--", sys.executable, "-c", "raise SystemExit(91)"], capture_output=True, timeout=3)
            self.assertEqual(result.returncode, 2)
