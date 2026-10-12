using Microsoft.Extensions.DependencyInjection;
using To_Do_list_app.Components.Shared;
using To_Do_list_app.Data;
using To_Do_list_app.Models;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class CategoriesCardRenderingTests
{
    [Fact]
    public async Task CategoriesCard_renders_all_five_categories_with_counts()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        await using (var db = harness.CreateContext())
        {
            db.Tasks.Add(MakeTask(tasks.SelectedDate, TaskCategory.Work));
            db.Tasks.Add(MakeTask(tasks.SelectedDate, TaskCategory.Work, TaskCategory.Health));
            await db.SaveChangesAsync();
        }

        var html = await ComponentHtml.RenderAsync<CategoriesCard>(
            new Dictionary<string, object?>(),
            services => services.AddScoped(_ => tasks));

        Assert.Contains("CATEGORIES", html);
        foreach (var label in new[] { "WORK", "PERSONAL", "HEALTH", "SHOPPING", "FITNESS" })
        {
            Assert.Contains(label, html);
        }
        Assert.Contains("aria-pressed=\"false\"", html);
        Assert.Contains("category-button-count", html);
    }

    [Fact]
    public async Task CategoriesCard_marks_the_active_filter_and_shows_reset()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        tasks.SetCategoryFilter(TaskCategory.Work);

        var html = await ComponentHtml.RenderAsync<CategoriesCard>(
            new Dictionary<string, object?>(),
            services => services.AddScoped(_ => tasks));

        Assert.Contains("aria-pressed=\"true\"", html);
        Assert.Contains("category-reset", html);
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