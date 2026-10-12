using Microsoft.Extensions.DependencyInjection;
using To_Do_list_app.Components.Shared;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class CalendarCardRenderingTests
{
    [Fact]
    public async Task CalendarCard_renders_the_current_month_grid()
    {
        using var harness = TestDb.Create();
        var html = await ComponentHtml.RenderAsync<CalendarCard>(
            new Dictionary<string, object?>(),
            services => services.AddScoped(_ => new TaskService(harness.Factory)));

        var today = DateOnly.FromDateTime(DateTime.Today);
        Assert.Contains(today.ToString("MMMM yyyy").ToUpperInvariant(), html);
        Assert.Equal(7, CountOccurrences(html, "calendar-weekday"));
        Assert.Equal(DateTime.DaysInMonth(today.Year, today.Month), CountOccurrences(html, "calendar-day"));
        Assert.Contains("Previous month", html);
        Assert.Contains("Next month", html);
    }

    [Fact]
    public async Task CalendarCard_marks_today_as_selected()
    {
        using var harness = TestDb.Create();
        var html = await ComponentHtml.RenderAsync<CalendarCard>(
            new Dictionary<string, object?>(),
            services => services.AddScoped(_ => new TaskService(harness.Factory)));

        Assert.Contains("is-selected", html);
        Assert.Contains("aria-current=\"date\"", html);
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