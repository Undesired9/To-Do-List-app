# My Daily Doodles

A hand-drawn paper-planner to-do app. Pick a day on the calendar, jot tasks onto
the sheet, tag them by category, and tick them off.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-SQLite-512BD4)
![Tests](https://img.shields.io/badge/tests-xUnit-512BD4)

Built as a Blazor Web App (Interactive Server) on .NET 10 with EF Core and SQLite.

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Getting started](#getting-started)
- [Database and migrations](#database-and-migrations)
- [Project structure](#project-structure)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Testing](#testing)

## Features

- **Daily task sheet** — tasks for the selected date in creation order, with a
  live "N tasks • M completed" summary and an empty state.
- **Quick add** — type a title and press Add. Blank titles are rejected with a
  screen-reader announcement.
- **Inline notes** — an optional note per task, addable, editable, and clearable
  from a collapsed editor on the row.
- **Category filter** — Work, Personal, Health, Shopping, Fitness. Tags render on
  each row; the categories card filters the list and shows per-category counts. A
  task counts in every category it carries.
- **Monthly calendar** — navigate months, jump to any day, and see markers on days
  that have tasks.
- **Persistence** — tasks are stored in SQLite; selected date and active category
  filter persist per browser via `ProtectedLocalStorage`.
- **Accessibility and polish** — aria labels, `aria-live` status regions, keyboard
  focus handling, reduced-motion support, and a responsive layout.

## Tech stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10 (`net10.0`) |
| UI | Blazor Web App, Interactive Server render mode |
| Data access | EF Core 10 with the SQLite provider |
| Database | SQLite (`app.db`), auto-migrated and seeded on startup |
| Testing | xUnit with `HtmlRenderer`-based component tests |

## Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) — developed against `10.0.302`
- Optional: [`dotnet-ef`](https://learn.microsoft.com/ef/core/cli/dotnet) for
  adding migrations

  ```powershell
  dotnet tool install --global dotnet-ef
  ```

### Run the app

```powershell
dotnet run
```

Open <http://localhost:5230>. On first run the app creates `app.db`, applies
migrations, and seeds seven sample tasks for the current day when the database
is empty. An HTTPS profile is also available at `https://localhost:7001`.

### Build

```powershell
dotnet build
```

## Database and migrations

The connection string lives in `appsettings.json`:

```json
"ConnectionStrings": { "PlannerDb": "Data Source=app.db" }
```

`Program.cs` calls `db.Database.MigrateAsync()` and `DbSeeder.SeedAsync(...)` at
startup, so a working database requires no manual step. The SQLite file is
git-ignored.

To evolve the schema:

```powershell
dotnet ef migrations add <Name>
dotnet ef database update
```

Existing migrations are in `Migrations/`.

## Project structure

```
Program.cs                     Startup: DI, EF Core factory, migrate + seed, routing
Components/
  App.razor                    HTML shell with CSS, JS, and font links
  Routes.razor                 Router, default layout, focus-on-navigate
  _Imports.razor               Shared usings for Razor components
  Layout/MainLayout.razor      Page layout
  Pages/Planner.razor          The "/" page; wires preferences to the task service
  Shared/                      UI components (see below)
Data/
  PlannerDbContext.cs          EF Core context and model configuration
  TaskItemCategory.cs          Join row linking a task to a category
  DbSeeder.cs                  Seeds sample tasks into an empty database
Models/
  TaskItem.cs                  Task entity
  TaskCategory.cs              Category enum
Services/
  TaskService.cs               Selected date, visible month, filter, task mutations
  PreferenceService.cs         Loads and saves per-browser preferences
  IPreferenceStore.cs          Storage seam, testable without a circuit
  ProtectedPreferenceStore.cs  ProtectedLocalStorage implementation
wwwroot/styles/                tokens, global, layout, and components CSS
tests/To-Do list app.Tests/    xUnit unit and component-render tests
```

### UI components (`Components/Shared`)

| Component | Responsibility |
|-----------|----------------|
| `AppHeader`, `AppFooter` | Branding and footer chrome |
| `TaskSheet` | Heading, counts, task list, and empty state |
| `TaskRow` | One task: checkbox, title, note, tags, delete |
| `TaskCheckbox` | Accessible completion toggle |
| `TaskNote` | Inline add, edit, and clear for the optional note |
| `CategoryTag` | Read-only `#Category` label |
| `DeleteTaskButton` | Delete action with an aria label |
| `CalendarCard` | Month grid, navigation, and task-day markers |
| `QuickAddCard` | New-task form |
| `CategoriesCard` | Filter buttons with counts and a clear action |

## Architecture

- **`TaskService` is scoped per Blazor circuit.** It owns the selected date,
  visible month, active category filter, and all task mutations. It raises
  `OnChange`; subscribing components refresh, and `Planner` persists preferences.
- **Short-lived `DbContext` instances.** Every operation pulls a fresh context
  from `IDbContextFactory<PlannerDbContext>`, so nothing long-lived tracks
  entities for the life of a circuit.
- **Preferences are client-side only.** Tasks are authoritative in SQLite; only
  `SelectedDate` and `ActiveCategory` are stored in the browser. Load and save
  failures (prerender, corrupt data) fall back to defaults.
- **Component tests render markup** with `HtmlRenderer`, verifying row, note, tag,
  calendar, and category output without a browser.

## Configuration

| File | Purpose |
|------|---------|
| `appsettings.json` | Logging, allowed hosts, and the `PlannerDb` connection string |
| `appsettings.Development.json` | Development overrides |
| `Properties/launchSettings.json` | `http` (5230) and `https` (7001) profiles |

## Testing

The test project is not referenced by the solution file, so point `dotnet test`
at it directly:

```powershell
dotnet test "tests/To-Do list app.Tests/To-Do list app.Tests.csproj"
```

Coverage includes task service behavior, navigation, seeding, category counts,
preference loading and saving, and component rendering for rows, notes, the
calendar, and the categories card.
