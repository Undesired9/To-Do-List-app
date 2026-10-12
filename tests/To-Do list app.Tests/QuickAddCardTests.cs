using Microsoft.Extensions.DependencyInjection;
using To_Do_list_app.Components.Shared;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class QuickAddCardTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void NormalizeTitle_rejects_blank_input(string raw)
    {
        Assert.Null(QuickAddCard.NormalizeTitle(raw));
        Assert.Null(QuickAddCard.NormalizeTitle(null));
    }

    [Fact]
    public void NormalizeTitle_trims_valid_input()
    {
        Assert.Equal("Buy groceries", QuickAddCard.NormalizeTitle("  Buy groceries  "));
    }

    [Fact]
    public async Task QuickAddCard_renders_labeled_input_and_live_region()
    {
        using var harness = TestDb.Create();
        var html = await ComponentHtml.RenderAsync<QuickAddCard>(
            new Dictionary<string, object?>(),
            services => services.AddScoped(_ => new TaskService(harness.Factory)));

        Assert.Contains("QUICK ADD", html);
        Assert.Contains("Write a task...", html);
        Assert.Contains("Task title", html);
        Assert.Contains("aria-live=\"polite\"", html);
    }

    [Fact]
    public async Task QuickAdd_whitespace_title_adds_nothing()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);

        await Assert.ThrowsAsync<ArgumentException>(() => tasks.AddTaskAsync("   "));

        Assert.Equal(0, await tasks.GetTotalCountAsync());
    }
}