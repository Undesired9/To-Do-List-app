using To_Do_list_app.Models;
using To_Do_list_app.Services;

namespace To_Do_list_app.Tests;

public class PreferenceServiceTests
{
    private sealed class FakePreferenceStore : IPreferenceStore
    {
        public Dictionary<string, object?> Items { get; } = new();
        public bool ThrowOnAccess { get; set; }

        public Task<T?> GetAsync<T>(string key)
        {
            if (ThrowOnAccess)
            {
                throw new InvalidOperationException("no circuit");
            }

            return Task.FromResult(Items.TryGetValue(key, out var value) && value is T typed ? typed : default);
        }

        public Task SetAsync<T>(string key, T value)
        {
            if (ThrowOnAccess)
            {
                throw new InvalidOperationException("no circuit");
            }

            Items[key] = value;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Load_restores_selected_date_and_filter()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        var store = new FakePreferenceStore();
        store.Items[PreferenceService.Key] = new Preferences("2026-10-26", "Work");
        var prefs = new PreferenceService(store);

        await prefs.LoadAsync(tasks);

        Assert.Equal(new DateOnly(2026, 10, 26), tasks.SelectedDate);
        Assert.Equal(TaskCategory.Work, tasks.ActiveCategory);
    }

    [Fact]
    public async Task Load_with_no_stored_data_keeps_defaults()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        var expectedDate = tasks.SelectedDate;
        var prefs = new PreferenceService(new FakePreferenceStore());

        await prefs.LoadAsync(tasks);

        Assert.Equal(expectedDate, tasks.SelectedDate);
        Assert.Null(tasks.ActiveCategory);
    }

    [Fact]
    public async Task Load_ignores_invalid_stored_data()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        var expectedDate = tasks.SelectedDate;
        var store = new FakePreferenceStore();
        store.Items[PreferenceService.Key] = new Preferences("not-a-date", "Nope");
        var prefs = new PreferenceService(store);

        await prefs.LoadAsync(tasks);

        Assert.Equal(expectedDate, tasks.SelectedDate);
        Assert.Null(tasks.ActiveCategory);
    }

    [Fact]
    public async Task Load_swallows_store_failures()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        var expectedDate = tasks.SelectedDate;
        var store = new FakePreferenceStore { ThrowOnAccess = true };
        var prefs = new PreferenceService(store);

        await prefs.LoadAsync(tasks);

        Assert.Equal(expectedDate, tasks.SelectedDate);
        Assert.Null(tasks.ActiveCategory);
    }

    [Fact]
    public async Task Save_writes_current_state_round_trip()
    {
        using var harness = TestDb.Create();
        var tasks = new TaskService(harness.Factory);
        tasks.SelectDate(new DateOnly(2026, 10, 26));
        tasks.SetCategoryFilter(TaskCategory.Work);
        var store = new FakePreferenceStore();
        var prefs = new PreferenceService(store);

        await prefs.SaveAsync(tasks);

        var stored = Assert.IsType<Preferences>(store.Items[PreferenceService.Key]);
        Assert.Equal("2026-10-26", stored.SelectedDate);
        Assert.Equal("Work", stored.ActiveCategory);
    }
}
