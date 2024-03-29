﻿using TerminalUI.ConsoleDrivers;
using TerminalUI.Controls;
using TerminalUI.Models;
using TerminalUI.TextFormat;

namespace TerminalUI.Examples.Controls;

public class ProgressBarExamples(IApplicationContext applicationContext)
{
    private static readonly IConsoleDriver _driver;

    static ProgressBarExamples()
    {
        _driver = new DotnetDriver();
        _driver.Init();
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
            ApplicationContext = applicationContext
        };

    private void RenderProgressBar<T>(ProgressBar<T> progressBar, Position position)
    {
        var s = new Size(10, 1);
        var renderContext = new RenderContext(
            _driver,
            true,
            null,
            null,
            new(),
            new TextFormatContext(true),
            new bool[s.Width,s.Height]
        );
        progressBar.Render(renderContext, position, s);
    }
}