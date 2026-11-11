# Security & git rules

## Secrets
- Connection strings with passwords and `Jwt:Key` live in **user-secrets** (dev) or environment variables (prod).
- Claude must not read/edit `appsettings.*.json`, `secrets.json`, `.env*`, `launchSettings.json` (enforced in `.claude/settings.json`). Tell the user the exact key/value to set instead.
- Never log tokens, passwords or connection strings.

## Git
- Commit messages: imperative, short subject (≤ 72 chars), e.g. `Add commission calculator with DB-driven rates`.
- **No AI attribution** in commits or PRs: no `Co-Authored-By: Claude`, no "Generated with Claude Code", no session links.
- Commit only when the user asks. Run `/pre-commit-check` first.
- Never commit `bin/`, `obj/`, user secrets, or `.env` files.
