// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using TerminalUI;
using TerminalUI.Color;
using TerminalUI.DependencyInjection;
using TerminalUI.Examples.Controls;
using TerminalUI.Examples.Mocks;
using TerminalUI.Styling;
using TerminalUI.Styling.Controls;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var services = new ServiceCollection()
    .AddTerminalUi()
    .AddSingleton<IRenderEngine, MockRenderEngine>();
IServiceProvider provider = services.BuildServiceProvider();

var applicationContext = provider.GetRequiredService<IApplicationContext>();
applicationContext.Theme = new Theme
{
    ControlThemes = new ControlThemes
    {
        ProgressBar = new ProgressBarTheme
        {
            ForegroundColor = ConsoleColors.Foregrounds.Blue
        }
    }
};

Console.CursorVisible = false;
new ProgressBarExamples(applicationContext).LoadingExample();
Console.CursorVisible = true;