using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TerminalUI.DependencyInjection;

public static class TerminalUiServiceCollectionExtensions
{
    public static IServiceCollection AddTerminalUi(this IServiceCollection collection)
    {
        collection.TryAddSingleton<IFocusManager, FocusManager>();
        collection.TryAddSingleton<IRenderEngine, RenderEngine>();
        collection.TryAddSingleton<IApplicationContext, ApplicationContext>();
        collection.TryAddSingleton<IEventLoop, EventLoop>();
        return collection;
    }
}
