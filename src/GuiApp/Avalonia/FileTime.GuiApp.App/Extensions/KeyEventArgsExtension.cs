using Avalonia.Input;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using GeneralInputKey;

namespace FileTime.GuiApp.App.Extensions;

public static class KeyEventArgsExtension
{
    public static GeneralKeyEventArgs? ToGeneralKeyEventArgs(
        this KeyEventArgs args,
        IAppKeyService<Key> appKeyService,
        SpecialKeysStatus specialKeysStatus)
    {
        var maybeKey = appKeyService.MapKey(args.Key);
        if (maybeKey is not { } key1) return null;

        var keyString = args.Key.ToString();
        return new GeneralKeyEventArgs(h => args.Handled = h)
        {
            Key = key1,
            KeyChar = keyString.Length > 0 ? keyString[0] : '\0',
            SpecialKeysStatus = specialKeysStatus
        };
    }

    public static GeneralKeyEventArgs? ToGeneralKeyEventArgs(
        this KeyEventArgs args,
        IAppKeyService<Key> appKeyService,
        KeyModifiers keyModifiers)
    {
        var maybeKey = appKeyService.MapKey(args.Key);
        if (maybeKey is not { } key1) return null;

        var keyString = args.Key.ToString();
        return new GeneralKeyEventArgs(h => args.Handled = h)
        {
            Key = key1,
            KeyChar = keyString.Length > 0 ? keyString[0] : '\0',
            SpecialKeysStatus = new SpecialKeysStatus(
                IsAltPressed: (keyModifiers & KeyModifiers.Alt) != 0,
                IsShiftPressed: (keyModifiers & KeyModifiers.Shift) != 0,
                IsCtrlPressed: (keyModifiers & KeyModifiers.Control) != 0
            )
        };
    }
}