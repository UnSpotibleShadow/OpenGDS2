# RC.4 Windows acceptance record

Status: **not run**. This records the existing M73 candidate; M74 does not create or validate a new package. Use the [owner checklist](early-access-testing.md) for all nineteen acceptance rows. A different ZIP requires its own record.

| Identity | Expected value |
| --- | --- |
| Visible version | `0.1.0-ea.1-rc.4` |
| Candidate ZIP bytes | `111831023` |
| ZIP SHA-256 | `8df091cb2c5026e63f18b8a345c068dbf6e3ecd6764d6167b7f1c78e724ee4b8` |
| Exported assembly SHA-256 | `d11676fb3fd1912cb10e942bf84459a07881de0259588582cc046cf4206068e4` |
| Provenance | Read source commit/export details from the candidate's `BUILD_MANIFEST.json`; do not substitute the M74 branch SHA |
| Availability | Prior handoff in issue #55; ZIP is absent from this continuation workspace |

On Windows, ZIP Properties supplies the byte count. For an optional hash check, open PowerShell in its folder and run `Get-FileHash -Algorithm SHA256 -LiteralPath '.\<candidate-name>.zip'`, replacing the placeholder with the actual filename. Record the result before extracting into a fresh folder.

Test record to complete:

- Tester/date: pending
- Windows version, display resolution/scaling, language: pending
- ZIP hash, manifest source commit and visible version: pending
- All nineteen checklist outcomes, with reproduction steps for failures: pending
- Blocking defects and issue links: pending
- Release decision for this exact candidate: pending

The current Python source tests do not satisfy native Windows acceptance. No checklist row is passed by this document. Import, save corruption, duplicate transactions, supported-loop dead ends and crashes block release under the existing checklist. Preserve the explicit Normal-mode progression limitation in the accepted release scope.
