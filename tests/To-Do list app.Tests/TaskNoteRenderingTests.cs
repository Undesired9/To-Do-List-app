using To_Do_list_app.Components.Shared;

namespace To_Do_list_app.Tests;

public class TaskNoteRenderingTests
{
    [Fact]
    public async Task TaskNote_with_empty_description_renders_no_note_line()
    {
        var html = await ComponentHtml.RenderAsync<TaskNote>(new Dictionary<string, object?>
        {
            ["TaskId"] = Guid.NewGuid(),
            ["Description"] = "   "
        });

        Assert.DoesNotContain("task-note-text", html);
    }

    [Fact]
    public async Task TaskNote_with_description_renders_the_note_line()
    {
        var html = await ComponentHtml.RenderAsync<TaskNote>(new Dictionary<string, object?>
        {
            ["TaskId"] = Guid.NewGuid(),
            ["Description"] = "Call about the electricity bill"
        });

        Assert.Contains("task-note-text", html);
        Assert.Contains("Call about the electricity bill", html);
    }

    [Fact]
    public async Task TaskNote_toggle_has_an_accessible_name()
    {
        var html = await ComponentHtml.RenderAsync<TaskNote>(new Dictionary<string, object?>
        {
            ["TaskId"] = Guid.NewGuid(),
            ["Description"] = null
        });

        Assert.Contains("Add note", html);
    }
}