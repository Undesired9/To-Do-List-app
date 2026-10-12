using Microsoft.EntityFrameworkCore;
using To_Do_list_app.Models;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task AddTask_persists_new_incomplete_task_for_selected_date()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);

        await service.AddTaskAsync("Walk the dog");

        await using var db = harness.CreateContext();
        var task = await db.Tasks.SingleAsync();
        Assert.Equal("Walk the dog", task.Title);
        Assert.False(task.Completed);
        Assert.Equal(service.SelectedDate, task.DueDate);
        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddTask_rejects_empty_or_whitespace_title(string title)
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddTaskAsync(title));
    }

    [Fact]
    public async Task ToggleTask_flips_completed()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        await service.AddTaskAsync("Task");
        var id = (await service.GetVisibleTasksAsync()).Single().Id;

        await service.ToggleTaskAsync(id);
        Assert.True((await service.GetVisibleTasksAsync()).Single().Completed);

        await service.ToggleTaskAsync(id);
        Assert.False((await service.GetVisibleTasksAsync()).Single().Completed);
    }

    [Fact]
    public async Task DeleteTask_removes_the_task()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        await service.AddTaskAsync("Task");
        var id = (await service.GetVisibleTasksAsync()).Single().Id;

        await service.DeleteTaskAsync(id);

        Assert.Empty(await service.GetVisibleTasksAsync());
    }

    [Fact]
    public async Task SetDescription_stores_trimmed_note()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        await service.AddTaskAsync("Task");
        var id = (await service.GetVisibleTasksAsync()).Single().Id;

        await service.SetDescriptionAsync(id, "  call about the invoice  ");

        Assert.Equal("call about the invoice", (await service.GetVisibleTasksAsync()).Single().Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SetDescription_normalizes_empty_to_null(string? note)
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        await service.AddTaskAsync("Task", "existing note");
        var id = (await service.GetVisibleTasksAsync()).Single().Id;

        await service.SetDescriptionAsync(id, note);

        Assert.Null((await service.GetVisibleTasksAsync()).Single().Description);
    }

    [Fact]
    public async Task Counts_reflect_selected_date_only()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var today = service.SelectedDate;

        await using (var db = harness.CreateContext())
        {
            db.Tasks.Add(new TaskItem { Title = "a", DueDate = today, Completed = true, CreatedAt = DateTime.UtcNow });
            db.Tasks.Add(new TaskItem { Title = "b", DueDate = today, Completed = false, CreatedAt = DateTime.UtcNow });
            db.Tasks.Add(new TaskItem { Title = "c", DueDate = today.AddDays(1), Completed = false, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        Assert.Equal(2, await service.GetTotalCountAsync());
        Assert.Equal(1, await service.GetCompletedCountAsync());
    }

    [Fact]
    public async Task AddTask_raises_OnChange()
    {
        using var harness = TestDb.Create();
        var service = new TaskService(harness.Factory);
        var raised = 0;
        service.OnChange += () => raised++;

        await service.AddTaskAsync("Task");

        Assert.Equal(1, raised);
    }
}
