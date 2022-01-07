using FileTime.App.Core;
using FileTime.App.Core.Clipboard;
using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.UI;
using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Command;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Core.StateManagement;
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
            /* Console.Clear();
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var i = x * 16 + y;
                    Console.Write("\u001b[48;5;{0}m{0,4}", i);
                    Console.ResetColor();
                    Console.Write(' ');
                }
                Console.WriteLine("\n");
            }
            return; */

            /* var colors = new int[][]
            {
                new int[] {0,43,54},
                new int[] {255,0,0},
                new int[] {0,255,0},
                new int[] {0,0,255},
            };

            foreach (var color in colors)
            {
                Console.Write($"\u001b[0m\u001b[48;2;{color[0]};{color[1]};{color[2]}mTESZT  ");
                Console.WriteLine($"\u001b[0m\u001b[38;2;{color[0]};{color[1]};{color[2]}mTESZT");
            }

            Console.WriteLine("\u001b[0m\u001b[48;5;0;38;5;14mASD");
            return; */

            var serviceProvider = CreateServiceProvider();
            _logger = serviceProvider.GetService<ILogger<Program>>()!;

            var coloredConsoleRenderer = serviceProvider.GetService<IColoredConsoleRenderer>()!;
            var localContentProvider = serviceProvider.GetService<LocalContentProvider>()!;

            var currentPath = Environment.CurrentDirectory.Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            _logger.LogInformation("Current directory: '{0}'", currentPath);
            var currentPossibleDirectory = localContentProvider.GetByPath(currentPath);

            if (currentPossibleDirectory is IContainer container)
            {
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