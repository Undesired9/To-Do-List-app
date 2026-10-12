namespace To_Do_list_app.Services;

/// <summary>Small seam over browser storage so preference logic can be tested without a circuit.</summary>
public interface IPreferenceStore
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value);
}
