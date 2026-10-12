using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace To_Do_list_app.Tests;

/// <summary>
/// Renders a component to static HTML so markup-level assertions (Review Focus #3, #4)
/// can run without a browser or bUnit.
/// </summary>
internal static class ComponentHtml
{
    public static async Task<string> RenderAsync<TComponent>(
        IDictionary<string, object?> parameters,
        Action<IServiceCollection>? configureServices = null)
        where TComponent : IComponent
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        configureServices?.Invoke(serviceCollection);
        var services = serviceCollection.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var view = ParameterView.FromDictionary(parameters);
        return await renderer.Dispatcher.InvokeAsync(() =>
        {
            var output = renderer.RenderComponentAsync<TComponent>(view).GetAwaiter().GetResult();
            return output.ToHtmlString();
        });
    }
}