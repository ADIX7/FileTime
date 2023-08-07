using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App.Extensions;

public static class KeyEventArgsExtensions
{
    public static GeneralKeyEventArgs? ToGeneralKeyEventArgs(this KeyEvent args, IAppKeyService<Key> appKeyService)
    {
        var maybeKey = appKeyService.MapKey(args.Key);
        if (maybeKey is not { } key1) return null;
        return new GeneralKeyEventArgs
        {
            Key = key1
        };
    }
}