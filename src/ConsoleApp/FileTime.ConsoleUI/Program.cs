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
        static Thread? _renderThread;

        public static async Task Main()
        {
            var serviceProvider = CreateServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Program>>()!;

            var coloredConsoleRenderer = serviceProvider.GetService<IColoredConsoleRenderer>()!;
            var localContentProvider = serviceProvider.GetService<LocalContentProvider>()!;
            var renderSynchronizer = serviceProvider.GetService<RenderSynchronizer>()!;

            var currentPath = Environment.CurrentDirectory.Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            _logger.LogInformation("Current directory: '{0}'", currentPath);
            var currentPossibleDirectory = await localContentProvider.GetByPath(currentPath);

            if (currentPossibleDirectory is IContainer container)
            {
                serviceProvider.GetService<TopContainer>();
                coloredConsoleRenderer.Clear();
                try
                {
                    Console.CursorVisible = false;
                }
                catch { }

                var app = serviceProvider.GetService<Application>()!;
                await app.SetContainer(container);
                renderSynchronizer.NeedsReRender();

                _renderThread = new Thread(new ThreadStart(renderSynchronizer.Start));
                _renderThread.Start();

                while (app.IsRunning)
                {
                    if (await app.ProcessKey(Console.ReadKey(true)))
                    {
                        renderSynchronizer.NeedsReRender();
                    }
                }

                renderSynchronizer.NeedsReRender();

                Console.SetCursorPosition(0, Console.WindowHeight - 1);

                Console.CursorVisible = true;
            }
            else
            {
                Console.WriteLine("Current working directory is not a directory??? Possible directory's type is: '" + currentPossibleDirectory?.GetType() + "'");
                Thread.Sleep(100);
            }
        }

        private static bool IsAnsiColorSupported()
        {
            try
            {
                Console.CursorLeft = 0;
                Console.CursorTop = 0;

                Console.Write("\u001b[0ma");

                return Console.CursorLeft == 1 && Console.CursorTop == 0;
            }
            catch
            {
                return false;
            }
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return DependencyInjection.RegisterDefaultServices()
                //.AddLogging()
                .AddLogging((builder) => builder.AddConsole().AddDebug())
                .AddSingleton<Application>()
                .AddSingleton<RenderSynchronizer>()
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