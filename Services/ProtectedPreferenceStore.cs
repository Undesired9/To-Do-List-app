using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace To_Do_list_app.Services;

/// <summary>
/// Stores preferences in the browser's protected local storage. Only usable
/// after the first interactive render; callers must treat failures as
/// "no stored value".
/// </summary>
public sealed class ProtectedPreferenceStore : IPreferenceStore
{
    private readonly ProtectedLocalStorage _storage;

    public ProtectedPreferenceStore(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var result = await _storage.GetAsync<T>(key);
        return result.Success ? result.Value : default;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await _storage.SetAsync(key, value);
    }
}
