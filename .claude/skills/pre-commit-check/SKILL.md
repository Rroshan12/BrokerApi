---
name: pre-commit-check
description: Gate before committing the House Broker API — build, test, format check, architecture review, and a commit message with no AI attribution. Use when the user asks to commit, push, or "check before commit".
---

# Pre-commit check

1. `dotnet build -warnaserror` — stop and report on failure.
2. `dotnet test` — stop and report on failure (with failing test names).
3. `dotnet format --verify-no-changes` — if it fails, run `dotnet format` and show what changed.
4. `git status --short` — confirm no `bin/`, `obj/`, secrets or `.env` files are staged.
5. Ask the `clean-arch-reviewer` subagent to review the diff; surface High findings before committing.
6. Propose a commit message: imperative subject ≤ 72 chars + optional body.
   **No AI attribution**: no `Co-Authored-By: Claude`, no "Generated with Claude Code", no session links.
7. Commit only if the user asked to commit; push only if they asked to push.

Report a short checklist: build ✓/✗, tests (passed/failed counts), format ✓/✗, review findings, proposed message.
