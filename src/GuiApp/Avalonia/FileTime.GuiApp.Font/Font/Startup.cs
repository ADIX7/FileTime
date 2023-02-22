using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.Font;

public static class Startup
{
    public static IServiceCollection ConfigureFont(this IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        services.Configure<FontConfiguration>(configurationRoot.GetSection(FontConfiguration.SectionName));
        services.AddSingleton<IFontService, FontService>();
        return services;
    }
}