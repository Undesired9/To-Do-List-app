# Frontend Specification --- My Daily Doodles

## 1. Overview

**Product:** My Daily Doodles\
**Type:** Responsive to-do list website\
**Primary experience:** A daily task planner styled like a bold,
hand-drawn paper notebook.

The task sheet is the primary focus. A compact right column contains a
monthly calendar, Quick Add, and Categories. Keep the interface
expressive but orderly: sketch details should never compromise
alignment, readability, or usability.

## 2. Recommended Frontend Stack

-   React + TypeScript for UI and state.
-   CSS Modules or organized CSS files for styling.
-   Lucide React or custom SVGs for utility icons.
-   Optional local storage for persistence in a frontend-only prototype.

These are recommendations, not requirements imposed by the design.

## 3. Design Tokens

### Exact palette

  -----------------------------------------------------------------------
  Token                   Value                   Use
  ----------------------- ----------------------- -----------------------
  `--color-bright-snow`   `#F8F9FA`               Main background and
                                                  light paper surfaces

  `--color-platinum`      `#E9ECEF`               Secondary paper layers,
                                                  muted surfaces, tags

  `--color-shadow-grey`   `#212529`               Text, borders, icons,
                                                  completed checkboxes
  -----------------------------------------------------------------------

Use these exact hex values. Do not substitute near-matching colors for
the core palette.

### Typography

-   Display/handwritten text: `Comic Neue`, with
    `"Comic Sans MS", cursive` as fallback.
-   Use a readable system sans-serif for dense body text if needed.
-   Brand and major headings can be bold and uppercase.
-   Keep small metadata legible; do not overuse decorative type.

### Sketch style

-   Primary outlines: approximately 3px solid `#212529`.
-   Secondary dividers: approximately 1--2px solid `#212529`.
-   Use offset duplicate paper layers, small tape strips, hand-drawn
    underlines, and restrained doodles.
-   Keep the content grid straight. Do not randomly rotate text,
    controls, or task rows.
-   Prefer offset paper shadows to generic soft SaaS shadows.
-   Use squared or lightly rounded paper corners.

## 4. Page Layout

Reference desktop frame: **1440 × 900 px**. Centered content width:
approximately **1040px**.

  Region                 Reference position/size
  ---------------------- ----------------------------------------------
  Header                 x=200, y=22, w=1040, h≈90
  Main content start     y=140
  Main task sheet        w≈730
  Gap between columns    30px
  Right utility column   w≈300
  Footer                 aligned to same 1040px container, near y=800

Use responsive CSS rather than relying on fixed desktop coordinates.

``` css
.page-shell {
  width: min(1040px, calc(100% - 40px));
  margin-inline: auto;
}

.main-layout {
  display: grid;
  grid-template-columns: minmax(0, 730px) minmax(260px, 300px);
  gap: 30px;
  align-items: start;
}
```

Page regions: 1. Header / brand and navigation 2. Main task paper 3.
Right utility column: calendar, Quick Add, Categories 4. Footer

Do not add a left sidebar, extra greeting panel, or redundant filter
bar.

## 5. Header

### Content

-   Hand-drawn notebook mark.
-   Brand: **MY DAILY DOODLES**
-   Navigation: My List, Calendar, Categories, Settings.
-   Profile area: circular avatar mark, "Hi, Alex!", and an arrow/logout
    icon.

### Behavior and layout

-   My List is the default active destination.
-   Calendar and Categories should navigate to or focus their
    corresponding sections.
-   Settings should open a settings view/panel if implemented.
-   Vertically center the brand, navigation, and profile.
-   Keep navigation spacing consistent and allow it to collapse/wrap on
    smaller screens.
-   Give icon-only controls accessible names.
-   Do not show logout as a functional control unless authentication
    exists.

## 6. Main Task Sheet

### Heading

-   Title: **TO-DO TODAY: OCTOBER 26**
-   Count: **7 tasks • 2 completed**

In a working app, generate the date and counts from state rather than
hard-coding them.

### Initial example tasks

  Task                                Categories           State
  ----------------------------------- -------------------- ------------
  Complete project proposal draft     Work, Health         Completed
  Go for a 30min run                  Health, Fitness      Completed
  Buy groceries (milk, eggs, bread)   Personal, Shopping   Completed
  Walk the dog                        Personal             Incomplete
  Team meeting at 2 PM                Work                 Incomplete
  Pay electricity bill                Personal             Incomplete
  Call Mom                            Personal             Incomplete

The category panel lists Work, Personal, Health, and Shopping. Fitness
appears in the sample task data; either support Fitness as an additional
category or normalize that sample tag to Health.

### Task row anatomy

Each row contains: 1. Checkbox 2. Task title 3. Category tag(s) 4.
Delete/trash action

### Task states

-   **Incomplete:** empty hand-drawn checkbox and normal title.
-   **Completed:** filled dark checkbox with check mark and crossed-out
    title.
-   **Hover:** subtle surface/outline change without layout movement.
-   **Keyboard focus:** visible high-contrast focus indicator.
-   **Delete:** remove the selected task; optionally provide undo.

### Alignment requirements

-   Use a consistent row grid for checkbox, title, tags, and delete
    action.
-   Let the title use flexible space.
-   Prevent tags and icons from shifting the checkbox alignment.
-   Aim for 40--44px hit targets for controls where practical.
-   Use separators sparingly so the paper outline remains dominant.

Small note at the bottom of the sheet: **✎ doodle it. do it. done.**
This is decorative, not a button.

## 7. Right Utility Column

Keep the three cards the same width, aligned to one left edge, with
consistent inner padding and border weights.

### 7.1 Monthly Calendar

-   Heading: **OCTOBER 2026**
-   Seven weekday columns and a standard monthly date grid.
-   Previous/next month controls.
-   Visually distinct selected day.
-   Optional marker for dates with tasks.

Generate the calendar grid from date logic. Use accessible buttons with
names such as "Previous month", "Next month", and "Select October 26".
Date cells must be evenly sized.

### 7.2 Quick Add

-   Card title: **QUICK ADD**
-   Text input with placeholder "Write a task..."
-   Add button; Enter can submit the form.

Behavior: - Trim whitespace. - Reject empty tasks. - Add the task, clear
the input, and announce the result to assistive technology. - If tasks
have dates, assign the currently selected date by default.

### 7.3 Categories

-   Heading: **CATEGORIES**
-   Labels: WORK, PERSONAL, HEALTH, SHOPPING.
-   Optional count per category.
-   Selecting a category filters/highlights matching tasks.
-   Show which filter is active and provide a clear way to reset it.

## 8. Footer

Align footer to the same 1040px container.

Text: **© 2026 MY DAILY DOODLES • Built with ♥ and coffee**

Use small but readable type and a simple divider or hand-drawn line.
Keep it visually quiet.

## 9. Functional Requirements

-   Add, complete, and delete tasks.
-   Display task category tags.
-   Automatically update total/completed counts.
-   Preserve task order unless sorting/reordering is implemented.
-   Navigate calendar months and select a date.
-   Reflect selected date in the heading and task query.
-   Filter by category and clear the filter.
-   Optionally persist tasks/preferences in local storage.
-   Handle invalid stored data safely and provide initial sample data.
-   Settings, if included, should remain lightweight: default view,
    delete confirmation, reduced motion, and reset demo data.

## 10. Suggested Data Model

``` ts
export type TaskCategory =
  | "Work"
  | "Personal"
  | "Health"
  | "Shopping"
  | "Fitness";

export interface Task {
  id: string;
  title: string;
  completed: boolean;
  categories: TaskCategory[];
  dueDate: string;   // YYYY-MM-DD
  createdAt: string; // ISO timestamp
}

export interface AppState {
  tasks: Task[];
  selectedDate: string; // YYYY-MM-DD
  activeCategory: TaskCategory | null;
}
```

Use stable task IDs, not array indexes.

## 11. Suggested Component Structure

``` text
src/
  components/
    AppHeader.tsx
    BrandMark.tsx
    Navigation.tsx
    ProfileControl.tsx
    TaskSheet.tsx
    TaskRow.tsx
    TaskCheckbox.tsx
    CategoryTag.tsx
    DeleteTaskButton.tsx
    CalendarCard.tsx
    QuickAddCard.tsx
    CategoriesCard.tsx
    AppFooter.tsx
  hooks/
    useTasks.ts
    useLocalStorage.ts
  types/
    task.ts
  styles/
    tokens.css
    global.css
    layout.css
    components.css
  App.tsx
  main.tsx
```

Treat this as a suggested organization, not a requirement to split every
small element into its own file.

## 12. Responsive Behavior

### Desktop: 1100px and wider

-   Centered 1040px content.
-   Task sheet and utility column side by side.
-   Approximately 730px / 300px columns with a 30px gap.

### Tablet: approximately 700--1099px

-   Reduce outer margins and column gap.
-   Stack the utility column below the task sheet if two columns become
    cramped.

### Mobile: below approximately 700px

-   Single-column layout.
-   Header navigation collapses or wraps.
-   Task sheet comes first; utility cards stack below.
-   Task titles can wrap without overlapping tags or delete controls.
-   Maintain comfortable touch targets.
-   Remove tape/offset decoration if it causes clipping or horizontal
    overflow.

``` css
@media (max-width: 760px) {
  .main-layout {
    grid-template-columns: minmax(0, 1fr);
    gap: 20px;
  }

  .utility-column {
    display: grid;
    grid-template-columns: minmax(0, 1fr);
    gap: 16px;
  }
}
```

## 13. Accessibility

-   Use semantic `header`, `nav`, `main`, `section`, and `footer`
    elements.
-   Use buttons for task toggles, date selection, delete, and month
    navigation.
-   Associate accessible labels with inputs.
-   Ensure all controls are keyboard reachable.
-   Provide visible focus indicators and sufficient contrast.
-   Do not communicate completion/selection through color alone.
-   Use `aria-pressed` or an equivalent accessible state for custom
    checkbox buttons.
-   Announce task changes where appropriate.
-   Hide purely decorative doodles from assistive technology.
-   Respect reduced-motion preferences.

## 14. Interaction and Motion

-   Checkbox animation: roughly 100--180ms.
-   Hover states may adjust the outline or surface subtly.
-   Keep paper layers static by default; avoid distracting parallax.
-   Reduced-motion mode removes nonessential animation.
-   Motion must not shift layout or cause neighboring controls to jump.

## 15. Acceptance Checklist

### Visual

-   [ ] Exact palette: `#F8F9FA`, `#E9ECEF`, `#212529`.
-   [ ] Bold hand-drawn paper-planner appearance.
-   [ ] Main task sheet is the dominant element.
-   [ ] Right cards share width and alignment.
-   [ ] Header, content, and footer share container edges.
-   [ ] Checkbox, title, tags, and delete action align consistently.
-   [ ] Offset paper layers and thick outlines feel intentional.
-   [ ] No unnecessary sidebar, greeting card, filter bar, or unrelated
    dashboard modules.
-   [ ] No horizontal overflow on mobile.

### Functional

-   [ ] Add, complete, and delete work.
-   [ ] Task counts update automatically.
-   [ ] Calendar navigation and date selection work.
-   [ ] Category filtering can be cleared.
-   [ ] Empty task input is rejected.
-   [ ] Persistence works if enabled.
-   [ ] Controls have accessible names and visible focus states.

### Code quality

-   [ ] Components are reusable where that improves consistency.
-   [ ] Layout responds to viewport size instead of relying on fixed
    desktop coordinates.
-   [ ] Task/date data are modeled explicitly.
-   [ ] Decorative effects do not interfere with readability or
    interaction.
-   [ ] No nonfunctional control is presented as working functionality.

## 16. Scope Notes

This document describes the frontend direction and expected
interactions. It does not imply that backend features already exist.
Authentication, cross-device sync, reminders, and server persistence are
out of scope unless separately implemented.

**Core principle:** bold sketch styling on top of a disciplined layout
grid. The page should look hand-drawn, never accidentally misaligned.
