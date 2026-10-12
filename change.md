# Change Log — My Daily Doodles

Running record of every decision and change made to this project. Newest
entries at the top within each date. Each entry captures what changed, why,
and what it affects, so any future session can reconstruct the history.

---

## 2026-10-12 — Project inception and planning

### Context

- Repository: `D:\Repos\To-Do list app` (was not a git repo at start).
- Starting state: minimal ASP.NET Core `net10.0` web app. `Program.cs` served
  `"Hello World!"` at `/`. No `wwwroot`, no frontend, no database.
- Existing design input: `frontend.md` (401-line design + interaction spec for
  "My Daily Doodles", a hand-drawn paper-planner to-do website).
- Toolchain confirmed: .NET SDK `10.0.302`; `blazor` template available.

### Decisions

| ID | Decision | Chosen | Alternatives considered | Rationale |
|----|----------|--------|-------------------------|-----------|
| D1 | Frontend stack | Blazor (Razor components) | Plain HTML/CSS/JS in `wwwroot`; React + TS | User selected Blazor; keeps the .NET toolchain and gives a component model without a JS build step. |
| D2 | Blazor hosting model | Blazor Web App — Interactive Server | Blazor WASM standalone; Interactive Auto | Single-page local-state planner; server interactivity works with `dotnet run`, no large WASM download. |
| D3 | Feature scope | Core + persistence | Core only; core + persistence + Settings + undo | Persistence is the one optional item tied to real acceptance behavior. Settings and undo deferred. |
| D4 | Backend data access | EF Core + SQLite, accessed directly from Blazor | EF Core behind a REST API; raw SQL (Dapper/Microsoft.Data.Sqlite) | Blazor Server can use EF Core via `IDbContextFactory` with no HTTP hop; migrations keep the schema sane. |
| D5 | Optional task description input | Inline expandable note per task | Description only in Quick Add; full inline edit mode | Keeps the spec's row grid intact while allowing add/edit any time. Description is optional. |

Scope change during brainstorm: the app moved from **frontend-only** (with
optional localStorage) to a **full-stack app** with a SQLite backend. Tasks
persist in SQLite; only lightweight UI preferences (selected date, active
category) persist per browser via `ProtectedLocalStorage`.

### Design summary (as approved)

- Blazor Web App, Interactive Server, .NET 10.
- `PlannerDbContext` (EF Core) with `TaskItem`, `TaskItemCategory` join table;
  SQLite file `app.db`.
- Seed the 7 `frontend.md` sample tasks on first run (empty DB).
- `TaskService` (scoped) owns operations via `IDbContextFactory`, raises
  `OnChange`; components subscribe.
- Components mirror `frontend.md` §11 (`AppHeader`, `TaskSheet`, `TaskRow`,
  `CalendarCard`, `QuickAddCard`, `CategoriesCard`, `AppFooter`, etc.).
- `TaskRow` gains a collapsed note icon for the optional description.
- Fitness kept as a supported 5th category (matches the spec data model).
- Exact palette `#F8F9FA`, `#E9ECEF`, `#212529`; Comic Neue + fallback; inline
  SVG icons; reduced-motion and focus/aria per spec.
- Deferred: Settings panel, undo-after-delete. Settings nav renders inactive,
  never a fake working control.

### Artifacts

- `frontend.md` — design + interaction spec (pre-existing; unchanged).
- `plan.md` — full implementation plan (task-by-task, TDD, 14 tasks).
- `change.md` — this file.

### Status

- Brainstorm complete; design approved by user.
- Plan written and self-reviewed.
- **No product code written yet.** Next step: user reviews `plan.md` and picks
  an execution method (subagent-driven or native), then implementation begins
  at Task 1.

### Open follow-ups

- Settings panel (default view, delete confirmation, reduced-motion toggle,
  reset demo data).
- Undo after delete.
- Auth, cross-device sync, reminders (out of scope per `frontend.md` §16).
- Git repo is not initialized in this folder yet; Task 1 initializes it.

---

<!--
Template for future entries:

## YYYY-MM-DD — <short title>

### Changed
- <file:what changed>

### Why
- <reason>

### Decisions
- <id if any>

### Verified
- <test/command + result>

### Status / open items
- <...>
-->
