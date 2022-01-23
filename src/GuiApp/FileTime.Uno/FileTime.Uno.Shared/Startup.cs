using FileTime.Uno.Application;
using FileTime.Uno.Services;
using FileTime.Uno.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno
{
    internal static class Startup
    {
        internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<AppState>()
                .AddTransient<MainPageViewModel>();
        }
        internal static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddLogging()
                .AddSingleton<ItemNameConverterService>();
        }
    }
}
