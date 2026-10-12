# My Daily Doodles — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a responsive "My Daily Doodles" to-do planner as a Blazor Web App (Interactive Server) backed by EF Core + SQLite, implementing the design in `frontend.md`.

**Architecture:** A Blazor Web App renders the single planner page server-side with interactive components. A scoped `TaskService` owns task state and talks to SQLite through `IDbContextFactory<PlannerDbContext>` (fresh context per operation, safe for long-lived circuits). Components subscribe to `TaskService.OnChange`. Tasks persist in SQLite; lightweight UI preferences (selected date, active category) persist per browser via `ProtectedLocalStorage`.

**Tech Stack:** .NET 10 (SDK 10.0.302), Blazor Web App Interactive Server, EF Core + SQLite (`Microsoft.EntityFrameworkCore.Sqlite`), xUnit for tests, hand-written CSS (no Node, no JS build step), inline SVG icons, Comic Neue via Google Fonts.

**Spec:** `frontend.md` (design + interaction spec). `change.md` (decision/change log).

---

## Global Constraints

Copy these verbatim into every task's work:

- Palette — exact hex only: `--color-bright-snow: #F8F9FA`, `--color-platinum: #E9ECEF`, `--color-shadow-grey: #212529`. No near-match substitutions.
- Typography: `Comic Neue`, fallback `"Comic Sans MS", cursive`. Readable system sans-serif for dense body text.
- Sketch style: primary outlines ≈3px solid `#212529`; secondary dividers 1–2px solid `#212529`; offset paper layers, small tape strips, hand-drawn underlines; squared/lightly rounded paper corners; offset paper shadows (not soft SaaS shadows). Content grid stays straight — no random rotation of text, controls, or rows.
- Layout: centered content width `min(1040px, calc(100% - 40px))`; two columns `minmax(0, 730px)` / `minmax(260px, 300px)` with `30px` gap; mobile single-column at `max-width: 760px`.
- Regions only: header, main task sheet, right utility column (calendar, quick add, categories), footer. No left sidebar, greeting panel, or redundant filter bar.
- Task row grid: checkbox, title (flex), category tags, delete. 40–44px hit targets where practical.
- Checkbox animation 100–180ms; respect `prefers-reduced-motion`; motion never shifts layout.
- Accessibility: semantic `header`/`nav`/`main`/`section`/`footer`; buttons for toggles/date/delete/month-nav; labeled inputs; visible focus indicators; `aria-pressed` for custom checkboxes; decorative doodles `aria-hidden`; no state conveyed by color alone.
- No nonfunctional control presented as working functionality.
- `.NET` nullable enabled; stable task IDs (Guid), never array indexes.
- Category enum: `Work, Personal, Health, Shopping, Fitness` (Fitness is a supported 5th category).

## Review Focus

Inputs/conditions the spec implies but no single task's happy path exercises. Each line has a test owned by the named task:

1. **Whitespace-only / empty title** submission must be rejected and the input preserved. — Task 11.
2. **Very long title on mobile** must wrap without overlapping tags or the delete control. — Task 13.
3. **Task with zero categories** must render cleanly (no stray tag or gap). — Task 8.
4. **Empty-string description** must normalize to `null` and never render a blank note line. — Task 9.
5. **Missing / empty database on startup** must seed the 7 sample tasks without throwing. — Task 2.

---

## File Structure

```
To-Do list app/
  Program.cs                          (modify) Blazor + EF + service wiring, migrate/seed
  To-Do list app.csproj               (modify) EF Core packages
  appsettings.json                    (modify) SQLite connection string
  Components/
    App.razor                         root HTML document
    Routes.razor                      router
    _Imports.razor                    shared usings
    Layout/MainLayout.razor           page shell
    Pages/Planner.razor               @page "/" — composes all regions
    Shared/
      AppHeader.razor  BrandMark.razor  Navigation.razor  ProfileControl.razor
      TaskSheet.razor  TaskRow.razor  TaskCheckbox.razor  CategoryTag.razor  DeleteTaskButton.razor
      CalendarCard.razor  QuickAddCard.razor  CategoriesCard.razor  AppFooter.razor
  Models/
    TaskCategory.cs                   enum
    TaskItem.cs                       entity (Title, Description?, Completed, DueDate, CreatedAt, Categories)
  Data/
    PlannerDbContext.cs               EF Core context + model config
    TaskItemCategory.cs               join entity (composite key)
    DbSeeder.cs                       sample-data seeding
  Services/
    TaskService.cs                    scoped state + operations + OnChange
    PreferenceService.cs              ProtectedLocalStorage prefs (selected date, filter)
  wwwroot/
    app.css                           (replace)
    styles/tokens.css  styles/global.css  styles/layout.css  styles/components.css
  tests/
    To-Do list app.Tests/
      To-Do list app.Tests.csproj
      TaskServiceTests.cs
      PreferenceServiceTests.cs (optional)
```

Task breakdown follows. Each task ends with a runnable, independently testable deliverable and a commit.

---

### Task 1: Convert project to Blazor Web App

**Files:**
- Modify: `To-Do list app.csproj`, `Program.cs`, `appsettings.json`
- Create: `Components/App.razor`, `Components/Routes.razor`, `Components/_Imports.razor`, `Components/Layout/MainLayout.razor`, `Components/Pages/Planner.razor`, `wwwroot/app.css`

**Interfaces:**
- Consumes: nothing.
- Produces: a runnable Blazor app serving `/` with `<Planner />`; `MainLayout` hosts `@Body`.

- [ ] **Step 1: Initialize git** (if not already a repo)

```bash
git init
git add -A
git commit -m "chore: baseline before Blazor conversion"
```

- [ ] **Step 2: Scaffold the Blazor files by hand** (avoid `dotnet new` overwriting the csproj)

Create the six files above. `Components/App.razor`:

```razor
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>My Daily Doodles</title>
  <base href="/" />
  <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Comic+Neue:wght@400;700&display=swap" />
  <link rel="stylesheet" href="app.css" />
  <link rel="stylesheet" href="styles/tokens.css" />
  <link rel="stylesheet" href="styles/global.css" />
  <link rel="stylesheet" href="styles/layout.css" />
  <link rel="stylesheet" href="styles/components.css" />
  <HeadOutlet />
</head>
<body>
  <Routes />
  <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

`Components/Routes.razor`:

```razor
<Router AppAssembly="typeof(Program).Assembly">
  <Found Context="routeData">
    <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
  </Found>
  <NotFound><p>Not found.</p></NotFound>
</Router>
```

`Components/_Imports.razor`:

```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.JSInterop
@using To_Do_list_app
@using To_Do_list_app.Components
@using To_Do_list_app.Components.Layout
@using To_Do_list_app.Components.Shared
@using To_Do_list_app.Models
@using To_Do_list_app.Services
```

`Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase
@Body
```

`Components/Pages/Planner.razor` (placeholder for now):

```razor
@page "/"
<h1>My Daily Doodles</h1>
```

- [ ] **Step 3: Wire Program.cs**

```csharp
using To_Do_list_app.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

- [ ] **Step 4: Run and verify**

Run: `dotnet run`
Expected: build succeeds; `http://localhost:<port>/` renders "My Daily Doodles".

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: convert to Blazor Web App shell"
```

---

### Task 2: Domain model, EF Core SQLite, migration, and seeding

**Files:**
- Create: `Models/TaskCategory.cs`, `Models/TaskItem.cs`, `Data/TaskItemCategory.cs`, `Data/PlannerDbContext.cs`, `Data/DbSeeder.cs`, `tests/To-Do list app.Tests/To-Do list app.Tests.csproj`
- Modify: `To-Do list app.csproj`, `Program.cs`, `appsettings.json`

**Interfaces:**
- Produces:
  - `enum TaskCategory { Work, Personal, Health, Shopping, Fitness }`
  - `class TaskItem { Guid Id; string Title; string? Description; bool Completed; DateOnly DueDate; DateTimeOffset CreatedAt; List<TaskItemCategory> Categories; }`
  - `class TaskItemCategory { Guid TaskItemId; TaskItem TaskItem; TaskCategory Category; }`
  - `class PlannerDbContext : DbContext { DbSet<TaskItem> Tasks; }`
  - `static class DbSeeder { static Task SeedAsync(PlannerDbContext db, DateOnly today); }`

- [ ] **Step 1: Add packages and connection string**

Add to csproj: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` (both 10.x). Add to `appsettings.json`:

```json
"ConnectionStrings": { "PlannerDb": "Data Source=app.db" }
```

Create the test project: `dotnet new xunit -o "tests/To-Do list app.Tests"` and add `Microsoft.EntityFrameworkCore.Sqlite` to it.

- [ ] **Step 2: Write the failing test — empty DB seeds 7 sample tasks**

```csharp
[Fact]
public async Task Seed_populates_seven_sample_tasks_when_empty()
{
    using var db = TestDb.New();            // file-backed temp SQLite, EnsureCreated
    await DbSeeder.SeedAsync(db, new DateOnly(2026, 10, 26));
    Assert.Equal(7, await db.Tasks.CountAsync());
    Assert.Equal(3, await db.Tasks.CountAsync(t => t.Completed));
    Assert.All(await db.Tasks.ToListAsync(), t => Assert.Equal(new DateOnly(2026,10,26), t.DueDate));
}
```

- [ ] **Step 3: Run to verify it fails**

Run: `dotnet test tests/To-Do list app.Tests`
Expected: FAIL — `PlannerDbContext` / `DbSeeder` not defined.

- [ ] **Step 4: Implement the entities, context, and seeder**

`OnModelCreating`: composite key `(TaskItemId, Category)`, store `Category` as string, `TaskItem.Title` required max 200, `Description` nullable max 1000, `DueDate` required. `DbSeeder` inserts the 7 spec sample tasks for `today` when `Tasks` is empty (Fitness kept on the run task). Add `TestDb.New()` helper creating a context on a temp file with `EnsureCreated()`.

- [ ] **Step 5: Wire factory + migrate/seed in Program.cs**

```csharp
builder.Services.AddDbContextFactory<PlannerDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("PlannerDb")));
// after build:
using (var scope = app.Services.CreateScope()) {
    var f = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PlannerDbContext>>();
    using var db = await f.CreateDbContextAsync();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, DateOnly.FromDateTime(DateTime.Today));
}
```

Create the migration: `dotnet tool install --global dotnet-ef` then `dotnet ef migrations add InitialCreate`. If the tool cannot be installed, call `db.Database.EnsureCreatedAsync()` instead and note it in `change.md`.

- [ ] **Step 6: Run to verify it passes**

Run: `dotnet test tests/To-Do list app.Tests`
Expected: PASS (all seed assertions).

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: EF Core SQLite model, migration, and sample seeding"
```

---

### Task 3: TaskService — core operations and counts

**Files:**
- Create: `Services/TaskService.cs`, `tests/To-Do list app.Tests/TaskServiceTests.cs`
- Modify: `Program.cs`

**Interfaces:**
- Consumes: `IDbContextFactory<PlannerDbContext>`, `TaskItem`.
- Produces: `class TaskService`
  - `event Action? OnChange;`
  - `DateOnly SelectedDate { get; }`, `DateOnly VisibleMonth { get; }`, `TaskCategory? ActiveCategory { get; }`
  - `Task<IReadOnlyList<TaskItem>> GetVisibleTasksAsync();`
  - `Task<int> GetTotalCountAsync(); Task<int> GetCompletedCountAsync();`
  - `Task AddTaskAsync(string title, string? description = null);`
  - `Task ToggleTaskAsync(Guid id); Task DeleteTaskAsync(Guid id);`
  - `Task SetDescriptionAsync(Guid id, string? description);`

- [ ] **Step 1: Write failing tests**

```csharp
[Fact] public async Task AddTask_persists_new_incomplete_task_for_selected_date();
[Fact] public async Task AddTask_rejects_empty_or_whitespace_title();
[Fact] public async Task ToggleTask_flips_completed();
[Fact] public async Task DeleteTask_removes_the_task();
[Fact] public async Task SetDescription_stores_trimmed_note();
[Fact] public async Task SetDescription_normalizes_empty_to_null();     // Review Focus #4
[Fact] public async Task Counts_reflect_selected_date_only();
[Fact] public async Task AddTask_raises_OnChange();
```

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test tests/To-Do list app.Tests --filter TaskServiceTests`
Expected: FAIL — `TaskService` not defined.

- [ ] **Step 3: Implement TaskService core**

Create a context per operation via `_factory.CreateDbContextAsync()`. `AddTaskAsync` trims the title, throws/ignores when empty (`ArgumentException`), trims description and converts whitespace-only to `null`, sets `DueDate = SelectedDate`, `CreatedAt = UtcNow`, persists, then `OnChange?.Invoke()`. `ToggleTaskAsync`/`DeleteTaskAsync`/`SetDescriptionAsync` load by Guid, mutate, save, raise `OnChange`. Counts query `Tasks` for `SelectedDate`. Register as scoped in `Program.cs`.

- [ ] **Step 4: Run to verify passing**

Run: `dotnet test tests/To-Do list app.Tests --filter TaskServiceTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: TaskService core task operations and counts"
```

---

### Task 4: TaskService — date navigation and category filter

**Files:**
- Modify: `Services/TaskService.cs`, `tests/To-Do list app.Tests/TaskServiceTests.cs`

**Interfaces:**
- Produces (additional members):
  - `void SelectDate(DateOnly date);`
  - `void NavigateMonth(int delta);`
  - `void SetCategoryFilter(TaskCategory? category);`
  - `void ClearFilter();`
  - `IReadOnlyList<DateOnly> GetTaskDatesInVisibleMonthAsync();` (dates with tasks, for calendar markers)

- [ ] **Step 1: Write failing tests**

```csharp
[Fact] public async Task SelectDate_changes_which_tasks_are_visible();
[Fact] public async Task NavigateMonth_moves_visible_month_and_clamps();
[Fact] public async Task SetCategoryFilter_narrows_visible_tasks();
[Fact] public async Task ClearFilter_restores_all_tasks_for_the_day();
[Fact] public async Task GetTaskDatesInVisibleMonth_returns_only_dates_with_tasks();
```

- [ ] **Step 2: Run to verify failure.** Run: `dotnet test --filter TaskServiceTests` → FAIL.

- [ ] **Step 3: Implement navigation and filtering.** `GetVisibleTasksAsync` returns tasks where `DueDate == SelectedDate`, ordered by `CreatedAt`, further filtered by `ActiveCategory` when set. Navigation clamps `VisibleMonth` to sane bounds and raises `OnChange`.

- [ ] **Step 4: Run to verify passing.** Expected: PASS.

- [ ] **Step 5: Commit.** `git commit -m "feat: TaskService date navigation and category filter"`

---

### Task 5: UI preference persistence

**Files:**
- Create: `Services/PreferenceService.cs`
- Modify: `Program.cs`, `Components/Pages/Planner.razor`

**Interfaces:**
- Produces: `class PreferenceService`
  - `Task LoadAsync(TaskService tasks);` (called after first render — ProtectedLocalStorage needs an active circuit)
  - `Task SaveAsync(TaskService tasks);`
  - serialized shape: `{ selectedDate: "YYYY-MM-DD", activeCategory: "Work|null" }`

- [ ] **Step 1:** Inject `ProtectedLocalStorage`; implement load/save with try/catch so corrupt data is ignored and falls back to today / no filter.
- [ ] **Step 2:** In `Planner.razor`, call `LoadAsync` in `OnAfterRenderAsync(firstRender: true)` and `SaveAsync` on `TaskService.OnChange`.
- [ ] **Step 3: Manual verify.** Run `dotnet run`; select a date and category, refresh, confirm they are restored.
- [ ] **Step 4: Commit.** `git commit -m "feat: persist selected date and filter per browser"`

---

### Task 6: Design tokens and global/layout CSS

**Files:**
- Create: `wwwroot/styles/tokens.css`, `wwwroot/styles/global.css`, `wwwroot/styles/layout.css`
- Replace: `wwwroot/app.css`

**Interfaces:** Produces CSS custom properties consumed by all components: `--color-bright-snow`, `--color-platinum`, `--color-shadow-grey`, `--font-hand`, `--font-body`, `--outline`, `--container`, `--radius`.

- [ ] **Step 1:** `tokens.css` defines the three exact hex values plus fonts, outline width, container width, radius.
- [ ] **Step 2:** `global.css`: reset, body background `--color-bright-snow`, base typography, `.visually-hidden`, focus-visible ring, `@media (prefers-reduced-motion: reduce)` killing animations.
- [ ] **Step 3:** `layout.css`: `.page-shell`, `.main-layout` grid `minmax(0,730px) / minmax(260px,300px)` gap 30px; `.utility-column` stack; mobile `@media (max-width:760px)` single column.
- [ ] **Step 4: Verify** page renders with correct background and centered shell.
- [ ] **Step 5: Commit.** `git commit -m "feat: design tokens and layout CSS"`

---

### Task 7: Header and footer

**Files:**
- Create: `Components/Shared/AppHeader.razor`, `BrandMark.razor`, `Navigation.razor`, `ProfileControl.razor`, `AppFooter.razor`
- Modify: `Components/Pages/Planner.razor`

**Interfaces:** `AppHeader` renders `BrandMark` ("MY DAILY DOODLES"), `Navigation` (My List active; Calendar/Categories focus sections; Settings rendered inactive/disabled), `ProfileControl` (avatar, "Hi, Alex!", arrow). `AppFooter` renders the copyright line.

- [ ] **Step 1:** Build header with a `<header>` + `<nav>`, semantic buttons, icon-only buttons given accessible names via `aria-label`; Settings is `disabled` with `aria-disabled="true"` (no fake working control).
- [ ] **Step 2:** Footer `<footer>` with divider and quiet type.
- [ ] **Step 3: Verify** header/footer align to the 1040px container.
- [ ] **Step 4: Commit.** `git commit -m "feat: header and footer"`

---

### Task 8: Task sheet, row, checkbox, tag, delete

**Files:**
- Create: `Components/Shared/TaskSheet.razor`, `TaskRow.razor`, `TaskCheckbox.razor`, `CategoryTag.razor`, `DeleteTaskButton.razor`
- Modify: `Components/Pages/Planner.razor`

**Interfaces:**
- `TaskSheet` consumes `TaskService`; heading `TO-DO TODAY: <MONTH DAY>` and `<n> tasks • <m> completed` from `GetTotalCountAsync`/`GetCompletedCountAsync`.
- `TaskRow` params: `TaskItem Task;`
- `TaskCheckbox` params: `bool Completed; EventCallback OnToggle;` — a `<button aria-pressed>` with hand-drawn box; filled + check when complete.
- `CategoryTag` param: `TaskCategory Category;`
- `DeleteTaskButton` params: `Guid TaskId; EventCallback OnDelete;` with `aria-label="Delete task"`.

- [ ] **Step 1: Write the failing test — zero-category task renders no tag** (Review Focus #3)

Render `TaskRow` for a task with `Categories` empty (bUnit or a component-level assertion helper); assert no `.category-tag` element is produced.

- [ ] **Step 2: Run to verify failure.** Expected: FAIL.
- [ ] **Step 3: Implement** the row grid (checkbox, title flex, tags, delete), completed state = filled checkbox + `text-decoration: line-through`, hover with no layout shift, focus-visible.
- [ ] **Step 4: Run to verify passing.** Expected: PASS.
- [ ] **Step 5: Commit.** `git commit -m "feat: task sheet, row, checkbox, tag, delete"`

---

### Task 9: Inline optional description note

**Files:**
- Modify: `Components/Shared/TaskRow.razor`, `wwwroot/styles/components.css`
- Create: `Components/Shared/TaskNote.razor`

**Interfaces:**
- `TaskNote` params: `Guid TaskId; string? Description; EventCallback<(Guid, string?)> OnSave;`
- Collapsed by default; a note icon button toggles a compact `<textarea>`/`<input>` under the title; saving calls `TaskService.SetDescriptionAsync`. A non-empty description renders as a muted line under the title when collapsed.

- [ ] **Step 1: Write the failing test — empty string normalizes to null and hides the note line** (Review Focus #4)
- [ ] **Step 2: Run to verify failure.** Expected: FAIL.
- [ ] **Step 3: Implement** the toggle + save; on save trim, whitespace-only → `null`; collapse when null/empty.
- [ ] **Step 4: Run to verify passing.** Expected: PASS.
- [ ] **Step 5: Commit.** `git commit -m "feat: inline optional task description"`

---

### Task 10: Calendar card

**Files:**
- Create: `Components/Shared/CalendarCard.razor`
- Modify: `wwwroot/styles/components.css`

**Interfaces:** `CalendarCard` consumes `TaskService` (`VisibleMonth`, `SelectedDate`, `GetTaskDatesInVisibleMonthAsync`); buttons `aria-label="Previous month"`, `"Next month"`, per-day `aria-label="Select <Month D>"`. Heading `OCTOBER 2026` style. Selected day visually distinct; dates with tasks marked.

- [ ] **Step 1:** Build the 7-column month grid from date logic (`DateTime.DaysInMonth`, first weekday), evenly sized cells.
- [ ] **Step 2: Verify** month nav and day selection update `TaskService` and the heading.
- [ ] **Step 3: Commit.** `git commit -m "feat: monthly calendar card"`

---

### Task 11: Quick Add card

**Files:**
- Create: `Components/Shared/QuickAddCard.razor`
- Modify: `wwwroot/styles/components.css`

**Interfaces:** `QuickAddCard` consumes `TaskService.AddTaskAsync`; text input placeholder `"Write a task..."`, Add button, Enter submits, labeled input.

- [ ] **Step 1: Write the failing test — empty/whitespace submission is rejected and input preserved** (Review Focus #1)
- [ ] **Step 2: Run to verify failure.** Expected: FAIL.
- [ ] **Step 3: Implement** trim → reject empty → add → clear input → announce via `aria-live="polite"`. Assign `SelectedDate` by default (handled in service).
- [ ] **Step 4: Run to verify passing.** Expected: PASS.
- [ ] **Step 5: Commit.** `git commit -m "feat: quick add card"`

---

### Task 12: Categories card

**Files:**
- Create: `Components/Shared/CategoriesCard.razor`
- Modify: `wwwroot/styles/components.css`

**Interfaces:** `CategoriesCard` consumes `TaskService`; labels `WORK`, `PERSONAL`, `HEALTH`, `SHOPPING`, `FITNESS`; per-category counts; `aria-pressed` on the active filter; a clear/reset control shown when a filter is active.

- [ ] **Step 1:** Render category buttons with counts; selecting sets the filter, selecting again or the reset clears it.
- [ ] **Step 2: Verify** filtering narrows the sheet and the active state + reset are visible.
- [ ] **Step 3: Commit.** `git commit -m "feat: categories card with filtering"`

---

### Task 13: Responsive, accessibility, and motion pass

**Files:**
- Modify: `wwwroot/styles/layout.css`, `components.css`, components as needed

- [ ] **Step 1: Write the failing test — very long title wraps without overlap on mobile** (Review Focus #2)

Add a CSS/render assertion (or a documented manual check with screenshot) that at `max-width: 760px` a long title wraps and the tag/delete controls remain reachable.

- [ ] **Step 2:** Apply mobile single-column layout, header collapse/wrap, remove tape/offset decoration if it overflows, keep 40–44px targets.
- [ ] **Step 3:** Confirm no horizontal overflow at 360px; focus states visible; reduced-motion disables nonessential animation.
- [ ] **Step 4: Commit.** `git commit -m "fix: responsive layout, a11y, and reduced motion"`

---

### Task 14: End-to-end acceptance verification

**Files:** none (verification only)

- [ ] **Step 1:** Run `dotnet run` and walk the `frontend.md` Section 15 acceptance checklist, visual + functional + code quality.
- [ ] **Step 2:** Confirm: add/complete/delete work; counts update; calendar nav/selection work; category filter clears; empty input rejected; persistence works; controls have accessible names and focus states; no horizontal overflow on mobile.
- [ ] **Step 3:** Record the outcome and any deviations in `change.md`.
- [ ] **Step 4: Commit.** `git commit -m "test: acceptance verification"`

---

## Self-Review

- **Spec coverage:** Every `frontend.md` section maps to a task — layout/tokens (T6), header/footer (T7), task sheet + states + description (T8–T9), calendar (T10), quick add (T11), categories (T12), responsive/a11y/motion (T13), functional requirements + acceptance (T3–T5, T14). Backend/SQLite persistence is the user's added requirement (T2–T5).
- **Step scan:** Steps are test/implement/run/commit; boilerplate files are given as exact copy because their wiring cannot be guessed.
- **Type consistency:** `TaskService` member names (`GetVisibleTasksAsync`, `GetTotalCountAsync`, `SetDescriptionAsync`) are used identically across T3–T13. `TaskItem`/`TaskCategory` names match T2 everywhere.
- **Review Focus:** Each of the five items names its owning task and test.
- **Proportion:** Signatures and test assertions only; bodies appear only where the spec fixes exact copy (T1 boilerplate, T2 schema).

## Open Follow-ups (deferred, not in this plan)

- Settings panel (default view, delete confirmation, reduced motion toggle, reset demo data).
- Undo after delete.
- Authentication, cross-device sync, reminders (explicitly out of scope in `frontend.md` §16).
