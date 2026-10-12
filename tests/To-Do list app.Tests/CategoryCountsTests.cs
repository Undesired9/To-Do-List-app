using To_Do_list_app.Data;
using To_Do_list_app.Models;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class CategoryCountsTests
{
    [Fact]
    public async Task GetCategoryCounts_groups_tasks_on_the_selected_date()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        await using (var db = harness.CreateContext())
        {
            db.Tasks.Add(MakeTask(tasks.SelectedDate, TaskCategory.Work));
            db.Tasks.Add(MakeTask(tasks.SelectedDate, TaskCategory.Work, TaskCategory.Health));
            db.Tasks.Add(MakeTask(tasks.SelectedDate.AddDays(1), TaskCategory.Work));
            await db.SaveChangesAsync();
        }

        var counts = await tasks.GetCategoryCountsAsync();

        Assert.Equal(2, counts[TaskCategory.Work]);
        Assert.Equal(1, counts[TaskCategory.Health]);
        Assert.False(counts.ContainsKey(TaskCategory.Shopping));
    }

    private static TaskItem MakeTask(DateOnly date, params TaskCategory[] categories)
    {
        var task = new TaskItem { Title = "task", DueDate = date, CreatedAt = DateTime.UtcNow };
        foreach (var category in categories)
        {
            task.Categories.Add(new TaskItemCategory { Category = category });
        }
        return task;
    }
}