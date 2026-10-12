using To_Do_list_app.Models;

namespace To_Do_list_app.Services;

/// <summary>Serialized shape of the persisted UI preferences.</summary>
public record Preferences(string SelectedDate, string? ActiveCategory);

/// <summary>
/// Loads and saves the selected date and active category filter per browser.
/// The task list itself lives in SQLite; only these view preferences are kept
/// client-side. Failures (no circuit yet, corrupt data) fall back to defaults.
/// </summary>
public class PreferenceService
{
    public const string Key = "planner.prefs";

    private readonly IPreferenceStore _store;

    public PreferenceService(IPreferenceStore store)
    {
        _store = store;
    }

    public async Task LoadAsync(TaskService tasks)
    {
        Preferences? prefs;
        try
        {
            prefs = await _store.GetAsync<Preferences>(Key);
        }
        catch
        {
            return;
        }

        if (prefs is null)
        {
            return;
        }

        if (DateOnly.TryParse(prefs.SelectedDate, out var date))
        {
            tasks.SelectDate(date);
        }

        if (Enum.TryParse<TaskCategory>(prefs.ActiveCategory, out var category))
        {
            tasks.SetCategoryFilter(category);
        }
        else
        {
            tasks.ClearFilter();
        }
    }

    public async Task SaveAsync(TaskService tasks)
    {
        var prefs = new Preferences(
            tasks.SelectedDate.ToString("yyyy-MM-dd"),
            tasks.ActiveCategory?.ToString());

        try
        {
            await _store.SetAsync(Key, prefs);
        }
        catch
        {
            // No interactive circuit (e.g. prerender): nothing to persist yet.
        }
    }
}
