using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Data;
using To_Do_list_app.Models;

namespace To_Do_list_app.Services;

/// <summary>
/// Scoped per Blazor circuit. Owns the selected date, visible month, active
/// category filter, and all task mutations. Each operation uses a short-lived
/// <see cref="PlannerDbContext"/> from the factory so no long-lived context
/// tracks state for the life of a circuit.
/// </summary>
public class TaskService
{
    private readonly IDbContextFactory<PlannerDbContext> _factory;

    public TaskService(IDbContextFactory<PlannerDbContext> factory)
    {
        _factory = factory;
        var today = DateOnly.FromDateTime(DateTime.Today);
        SelectedDate = today;
        VisibleMonth = new DateOnly(today.Year, today.Month, 1);
    }

    public event Action? OnChange;

    public DateOnly SelectedDate { get; private set; }

    /// <summary>First day of the month the calendar is showing.</summary>
    public DateOnly VisibleMonth { get; private set; }

    public TaskCategory? ActiveCategory { get; private set; }

    /// <summary>Tasks due on the selected date, filtered by the active category, in creation order.</summary>
    public async Task<IReadOnlyList<TaskItem>> GetVisibleTasksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.Tasks
            .Include(t => t.Categories)
            .Where(t => t.DueDate == SelectedDate);

        if (ActiveCategory is { } category)
        {
            query = query.Where(t => t.Categories.Any(c => c.Category == category));
        }

        return await query.OrderBy(t => t.CreatedAt).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks.CountAsync(t => t.DueDate == SelectedDate);
    }

    public async Task<int> GetCompletedCountAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks.CountAsync(t => t.DueDate == SelectedDate && t.Completed);
    }

    public async Task AddTaskAsync(string title, string? description = null)
    {
        var trimmed = (title ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        await using var db = await _factory.CreateDbContextAsync();
        db.Tasks.Add(new TaskItem
        {
            Title = trimmed,
            Description = Normalize(description),
            DueDate = SelectedDate,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        OnChange?.Invoke();
    }

    public async Task ToggleTaskAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return;
        }

        task.Completed = !task.Completed;
        await db.SaveChangesAsync();
        OnChange?.Invoke();
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return;
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        OnChange?.Invoke();
    }

    public async Task SetDescriptionAsync(Guid id, string? description)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return;
        }

        task.Description = Normalize(description);
        await db.SaveChangesAsync();
        OnChange?.Invoke();
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public void SelectDate(DateOnly date)
    {
        SelectedDate = date;
        VisibleMonth = new DateOnly(date.Year, date.Month, 1);
        OnChange?.Invoke();
    }

    /// <summary>Moves the visible month by <paramref name="delta"/> months. A zero delta is a no-op.</summary>
    public void NavigateMonth(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        VisibleMonth = VisibleMonth.AddMonths(delta);
        OnChange?.Invoke();
    }

    public void SetCategoryFilter(TaskCategory? category)
    {
        ActiveCategory = category;
        OnChange?.Invoke();
    }

    public void ClearFilter()
    {
        ActiveCategory = null;
        OnChange?.Invoke();
    }

    /// <summary>Number of tasks on the selected date per category (a task counts in each of its categories).</summary>
    public async Task<IReadOnlyDictionary<TaskCategory, int>> GetCategoryCountsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var counts = await db.Tasks
            .Where(t => t.DueDate == SelectedDate)
            .SelectMany(t => t.Categories)
            .GroupBy(c => c.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.Category, x => x.Count);
    }

    /// <summary>Distinct dates within the visible month that have at least one task, for calendar markers.</summary>
    public async Task<IReadOnlyList<DateOnly>> GetTaskDatesInVisibleMonthAsync()
    {
        var first = VisibleMonth;
        var last = first.AddMonths(1);

        await using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks
            .Where(t => t.DueDate >= first && t.DueDate < last)
            .Select(t => t.DueDate)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}
