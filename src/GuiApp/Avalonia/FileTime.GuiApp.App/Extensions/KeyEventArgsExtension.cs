using Avalonia.Input;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;

namespace FileTime.GuiApp.App.Extensions;

public static class KeyEventArgsExtension
{
    public static GeneralKeyEventArgs? ToGeneralKeyEventArgs(this KeyEventArgs args, IAppKeyService<Key> appKeyService)
    {
        var maybeKey = appKeyService.MapKey(args.Key);
        if (maybeKey is not {} key1) return null;
        return new GeneralKeyEventArgs(h => args.Handled = h)
        {
            Key = key1
        };
    }
}