using FileTime.GuiApp.App.Configuration;
using FileTime.GuiApp.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.App.Font;

public static class Startup
{
    public static IServiceCollection ConfigureFont(this IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        services.AddOptions<FontConfiguration>().Bind(configurationRoot.GetSection(FontConfiguration.SectionName));
        services.AddSingleton<IFontService, FontService>();
        return services;
    }
}