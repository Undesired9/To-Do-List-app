namespace To_Do_list_app.Tests;

/// <summary>
/// Structural guards for the responsive/a11y pass. Layout itself is verified
/// with a real screenshot at the acceptance step (Task 14).
/// </summary>
public class ResponsiveCssTests
{
    [Fact]
    public void Layout_stacks_to_one_column_on_mobile()
    {
        var css = ReadRepoFile("wwwroot/styles/layout.css");

        Assert.Contains("@media (max-width: 760px)", css);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", css);
    }

    // Review Focus #2: a very long title must wrap instead of overlapping tags/delete on mobile.
    [Fact]
    public void Task_title_can_wrap_and_mobile_row_keeps_controls_reachable()
    {
        var css = ReadRepoFile("wwwroot/styles/components.css");

        Assert.Contains("overflow-wrap: anywhere", css);
        Assert.Contains("grid-template-areas", css);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "To-Do list app.csproj")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, relativePath));
    }
}