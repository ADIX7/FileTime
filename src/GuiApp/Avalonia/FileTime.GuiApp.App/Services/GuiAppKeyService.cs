using System.Collections.ObjectModel;
using Avalonia.Input;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using GeneralInputKey;

namespace FileTime.GuiApp.App.Services;

public sealed class GuiAppKeyService : IAppKeyService<Key>
{
    private static readonly Dictionary<Key, Keys> KeyMapping;

    //TODO: write test for this. Test if every enum value is present in the dictionary.
    public static ReadOnlyDictionary<Key, Keys> KeyMappingReadOnly { get; }

    static GuiAppKeyService()
    {
        KeyMapping = new Dictionary<Key, Keys>
        {
            {Key.A, Keys.A},
            {Key.B, Keys.B},
            {Key.C, Keys.C},
            {Key.D, Keys.D},
            {Key.E, Keys.E},
            {Key.F, Keys.F},
            {Key.G, Keys.G},
            {Key.H, Keys.H},
            {Key.I, Keys.I},
            {Key.J, Keys.J},
            {Key.K, Keys.K},
            {Key.L, Keys.L},
            {Key.M, Keys.M},
            {Key.N, Keys.N},
            {Key.O, Keys.O},
            {Key.P, Keys.P},
            {Key.Q, Keys.Q},
            {Key.R, Keys.R},
            {Key.S, Keys.S},
            {Key.T, Keys.T},
            {Key.U, Keys.U},
            {Key.V, Keys.V},
            {Key.W, Keys.W},
            {Key.X, Keys.X},
            {Key.Y, Keys.Y},
            {Key.Z, Keys.Z},
            {Key.F1, Keys.F1},
            {Key.F2, Keys.F2},
            {Key.F3, Keys.F3},
            {Key.F4, Keys.F4},
            {Key.F5, Keys.F5},
            {Key.F6, Keys.F6},
            {Key.F7, Keys.F7},
            {Key.F8, Keys.F8},
            {Key.F9, Keys.F9},
            {Key.F10, Keys.F10},
            {Key.F11, Keys.F11},
            {Key.F12, Keys.F12},
            {Key.D0, Keys.Num0},
            {Key.D1, Keys.Num1},
            {Key.D2, Keys.Num2},
            {Key.D3, Keys.Num3},
            {Key.D4, Keys.Num4},
            {Key.D5, Keys.Num5},
            {Key.D6, Keys.Num6},
            {Key.D7, Keys.Num7},
            {Key.D8, Keys.Num8},
            {Key.D9, Keys.Num9},
            {Key.Up, Keys.Up},
            {Key.Down, Keys.Down},
            {Key.Left, Keys.Left},
            {Key.Right, Keys.Right},
            {Key.Enter, Keys.Enter},
            {Key.Escape, Keys.Escape},
            {Key.Back, Keys.Backspace},
            {Key.Delete, Keys.Delete},
            {Key.Space, Keys.Space},
            {Key.PageUp, Keys.PageUp},
            {Key.PageDown, Keys.PageDown},
            {Key.OemComma, Keys.Comma},
            {Key.OemQuestion, Keys.Question},
            {Key.Tab, Keys.Tab},
            {Key.LWin, Keys.LWin},
            {Key.RWin, Keys.RWin},
        };

        KeyMappingReadOnly = new(KeyMapping);
    }

    public Keys? MapKey(Key key)
    {
        if (!KeyMapping.TryGetValue(key, out var mappedKey)) return null;
        return mappedKey;
    }
}