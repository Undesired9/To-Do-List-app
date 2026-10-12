using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Data;
using To_Do_list_app.Models;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class TaskServiceNavigationTests
{
    private static async Task InsertAsync(TestDb harness, string title, DateOnly due, params TaskCategory[] categories)
    {
        await using var db = harness.CreateContext();
        db.Tasks.Add(new TaskItem
        {
            Title = title,
            DueDate = due,
            CreatedAt = DateTime.UtcNow,
            Categories = categories.Select(c => new TaskItemCategory { Category = c }).ToList()
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task SelectDate_changes_which_tasks_are_visible()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var today = service.SelectedDate;
        await InsertAsync(harness, "today", today);
        await InsertAsync(harness, "tomorrow", today.AddDays(1));

        service.SelectDate(today.AddDays(1));

        var visible = await service.GetVisibleTasksAsync();
        Assert.Single(visible);
        Assert.Equal("tomorrow", visible[0].Title);
    }

    [Fact]
    public void NavigateMonth_moves_visible_month_and_keeps_it_first_of_month()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var start = service.VisibleMonth;

        service.NavigateMonth(1);
        Assert.Equal(start.AddMonths(1), service.VisibleMonth);
        Assert.Equal(1, service.VisibleMonth.Day);

        service.NavigateMonth(-1);
        Assert.Equal(start, service.VisibleMonth);

        var raised = 0;
        service.OnChange += () => raised++;
        service.NavigateMonth(0);
        Assert.Equal(0, raised);
    }

    [Fact]
    public async Task SetCategoryFilter_narrows_visible_tasks()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var today = service.SelectedDate;
        await InsertAsync(harness, "work item", today, TaskCategory.Work);
        await InsertAsync(harness, "personal item", today, TaskCategory.Personal);

        service.SetCategoryFilter(TaskCategory.Work);

        var visible = await service.GetVisibleTasksAsync();
        Assert.Single(visible);
        Assert.Equal("work item", visible[0].Title);
    }

    [Fact]
    public async Task ClearFilter_restores_all_tasks_for_the_day()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var today = service.SelectedDate;
        await InsertAsync(harness, "work item", today, TaskCategory.Work);
        await InsertAsync(harness, "personal item", today, TaskCategory.Personal);
        service.SetCategoryFilter(TaskCategory.Work);

        service.ClearFilter();

        Assert.Equal(2, (await service.GetVisibleTasksAsync()).Count);
        Assert.Null(service.ActiveCategory);
    }

    [Fact]
    public async Task GetTaskDatesInVisibleMonth_returns_only_dates_with_tasks()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var first = service.VisibleMonth;
        await InsertAsync(harness, "day 3", first.AddDays(2));
        await InsertAsync(harness, "day 10", first.AddDays(9));
        await InsertAsync(harness, "next month", first.AddMonths(1));

        var dates = await service.GetTaskDatesInVisibleMonthAsync();

        Assert.Equal(2, dates.Count);
        Assert.Contains(first.AddDays(2), dates);
        Assert.Contains(first.AddDays(9), dates);
        Assert.DoesNotContain(first.AddMonths(1), dates);
    }
}
