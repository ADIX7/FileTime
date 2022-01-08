using FileTime.App.Core;
using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.UI;
using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Command;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.ConsoleUI
{
    public class Program
    {
        static ILogger<Program>? _logger;

        public static void Main()
        {
            var serviceProvider = CreateServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Program>>()!;

            var coloredConsoleRenderer = serviceProvider.GetService<IColoredConsoleRenderer>()!;
            var localContentProvider = serviceProvider.GetService<LocalContentProvider>()!;

            var currentPath = Environment.CurrentDirectory.Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            _logger.LogInformation("Current directory: '{0}'", currentPath);
            var currentPossibleDirectory = localContentProvider.GetByPath(currentPath);

            if (currentPossibleDirectory is IContainer container)
            {
                serviceProvider.GetService<TopContainer>();
                coloredConsoleRenderer.Clear();
                Console.CursorVisible = false;

                var app = serviceProvider.GetService<Application>()!;
                app.SetContainer(container);
                app.PrintUI();

                while (app.IsRunning)
                {
                    if (app.ProcessKey(Console.ReadKey(true)))
                    {
                        app.PrintUI();
                    }
                }

                Console.SetCursorPosition(0, Console.WindowHeight - 1);

                Console.CursorVisible = true;
            }
            else
            {
                Console.WriteLine("Current working directory is not a directory???");
                Thread.Sleep(100);
            }
        }

        private static bool IsAnsiColorSupported()
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            Console.Write("\u001b[0ma");

            return Console.CursorLeft == 1 && Console.CursorTop == 0;
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return DependencyInjection.RegisterDefaultServices()
                .AddLogging(/* (builder) => builder.AddConsole().AddDebug() */)
                .AddSingleton<Application>()
                .AddSingleton<IStyles>(new Styles(IsAnsiColorSupported()))
                .AddSingleton<IColoredConsoleRenderer, ColoredConsoleRenderer>()
                .AddSingleton<ConsoleReader>()
                .AddSingleton<IInputInterface, ConsoleInputInterface>()

                .AddTransient<Render>()
                .RegisterCommandHandlers()
                .BuildServiceProvider();
        }
    }

    internal static class ProgramExtensions
    {
        internal static IServiceCollection RegisterCommandHandlers(this IServiceCollection serviceCollection)
        {
            foreach (var commandHandler in Startup.GetCommandHandlers())
            {
                serviceCollection.AddTransient(typeof(ICommandHandler), commandHandler);
            }

            return serviceCollection;
        }
    }
}