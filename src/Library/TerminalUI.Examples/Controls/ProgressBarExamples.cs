using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Controls;
using TerminalUI.Models;
using TerminalUI.TextFormat;

namespace TerminalUI.Examples.Controls;

public class ProgressBarExamples
{
    private readonly IApplicationContext _applicationContext;
    private static readonly IConsoleDriver _driver;

    static ProgressBarExamples()
    {
        _driver = new DotnetDriver();
        _driver.Init();
    }

    public ProgressBarExamples(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public void LoadingExample()
    {
        var progressBar = CreateProgressBar<object>(0);
        for (var i = 0; i < 100; i++)
        {
            progressBar.Value = i;
            RenderProgressBar(progressBar, new Position(0, 0));
            Thread.Sleep(100);
        }
    }

    public void PaletteExample()
    {
        RenderProgressBar(CreateProgressBar<object>(100), new Position(0, 0));
        for (var i = 0; i < 10; i++)
        {
            RenderProgressBar(CreateProgressBar<object>(10 * (i + 1)), new Position(0, i + 1));
        }
    }

    private ProgressBar<T> CreateProgressBar<T>(int percent) =>
        new()
        {
            Value = percent,
            Attached = true,
            ApplicationContext = _applicationContext
        };

    private void RenderProgressBar<T>(ProgressBar<T> progressBar, Position position)
    {
        var renderContext = new RenderContext(
            _driver,
            true,
            null,
            null,
            new(),
            new TextFormatContext(true)
        );
        progressBar.Render(renderContext, position, new Size(10, 1));
    }
}