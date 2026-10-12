using To_Do_list_app.Components.Shared;
using To_Do_list_app.Data;
using To_Do_list_app.Models;

namespace To_Do_list_app.Tests;

public class TaskRowRenderingTests
{
    [Fact]
    public async Task TaskRow_with_no_categories_renders_no_tag()
    {
        var html = await ComponentHtml.RenderAsync<TaskRow>(new Dictionary<string, object?>
        {
            ["Task"] = new TaskItem { Title = "Walk the dog" }
        });

        Assert.False(html.Contains("category-tag", StringComparison.Ordinal));
        Assert.Contains("Walk the dog", html);
    }

    [Fact]
    public async Task TaskRow_renders_one_tag_per_category()
    {
        var html = await ComponentHtml.RenderAsync<TaskRow>(new Dictionary<string, object?>
        {
            ["Task"] = new TaskItem
            {
                Title = "Team meeting at 2 PM",
                Categories = new List<TaskItemCategory> { new() { Category = TaskCategory.Work } }
            }
        });

        Assert.Contains("#Work", html);
        Assert.Equal(1, CountOccurrences(html, "category-tag"));
    }

    [Fact]
    public async Task TaskRow_checkbox_exposes_aria_pressed_state()
    {
        var html = await ComponentHtml.RenderAsync<TaskRow>(new Dictionary<string, object?>
        {
            ["Task"] = new TaskItem { Title = "Go for a 30min run", Completed = true }
        });

        Assert.Contains("aria-pressed=\"true\"", html);
    }

    [Fact]
    public async Task TaskRow_checkbox_aria_label_includes_the_title()
    {
        var html = await ComponentHtml.RenderAsync<TaskRow>(new Dictionary<string, object?>
        {
            ["Task"] = new TaskItem { Title = "Pay electricity bill" }
        });

        Assert.Contains("Mark task completed: Pay electricity bill", html);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }
}