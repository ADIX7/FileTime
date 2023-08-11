using System.Collections.ObjectModel;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using GeneralInputKey;

namespace FileTime.ConsoleUI.App.KeyInputHandling;

public class ConsoleAppKeyService : IAppKeyService<ConsoleKey>
{
    private static readonly Dictionary<ConsoleKey, Keys> KeyMapping;

    //TODO: write test for this. Test if every enum value is present in the dictionary.
    public static ReadOnlyDictionary<ConsoleKey, Keys> KeyMappingReadOnly { get; }

    static ConsoleAppKeyService()
    {
        KeyMapping = new Dictionary<ConsoleKey, Keys>
        {
            {ConsoleKey.A, Keys.A},
            {ConsoleKey.B, Keys.B},
            {ConsoleKey.C, Keys.C},
            {ConsoleKey.D, Keys.D},
            {ConsoleKey.E, Keys.E},
            {ConsoleKey.F, Keys.F},
            {ConsoleKey.G, Keys.G},
            {ConsoleKey.H, Keys.H},
            {ConsoleKey.I, Keys.I},
            {ConsoleKey.J, Keys.J},
            {ConsoleKey.K, Keys.K},
            {ConsoleKey.L, Keys.L},
            {ConsoleKey.M, Keys.M},
            {ConsoleKey.N, Keys.N},
            {ConsoleKey.O, Keys.O},
            {ConsoleKey.P, Keys.P},
            {ConsoleKey.Q, Keys.Q},
            {ConsoleKey.R, Keys.R},
            {ConsoleKey.S, Keys.S},
            {ConsoleKey.T, Keys.T},
            {ConsoleKey.U, Keys.U},
            {ConsoleKey.V, Keys.V},
            {ConsoleKey.W, Keys.W},
            {ConsoleKey.X, Keys.X},
            {ConsoleKey.Y, Keys.Y},
            {ConsoleKey.Z, Keys.Z},
            {ConsoleKey.F1, Keys.F1},
            {ConsoleKey.F2, Keys.F2},
            {ConsoleKey.F3, Keys.F3},
            {ConsoleKey.F4, Keys.F4},
            {ConsoleKey.F5, Keys.F5},
            {ConsoleKey.F6, Keys.F6},
            {ConsoleKey.F7, Keys.F7},
            {ConsoleKey.F8, Keys.F8},
            {ConsoleKey.F9, Keys.F9},
            {ConsoleKey.F10, Keys.F10},
            {ConsoleKey.F11, Keys.F11},
            {ConsoleKey.F12, Keys.F12},
            {ConsoleKey.D0, Keys.Num0},
            {ConsoleKey.D1, Keys.Num1},
            {ConsoleKey.D2, Keys.Num2},
            {ConsoleKey.D3, Keys.Num3},
            {ConsoleKey.D4, Keys.Num4},
            {ConsoleKey.D5, Keys.Num5},
            {ConsoleKey.D6, Keys.Num6},
            {ConsoleKey.D7, Keys.Num7},
            {ConsoleKey.D8, Keys.Num8},
            {ConsoleKey.D9, Keys.Num9},
            {ConsoleKey.UpArrow, Keys.Up},
            {ConsoleKey.DownArrow, Keys.Down},
            {ConsoleKey.LeftArrow, Keys.Left},
            {ConsoleKey.RightArrow, Keys.Right},
            {ConsoleKey.Enter, Keys.Enter},
            {ConsoleKey.Escape, Keys.Escape},
            {ConsoleKey.Backspace, Keys.Backspace},
            {ConsoleKey.Delete, Keys.Delete},
            {ConsoleKey.Spacebar, Keys.Space},
            {ConsoleKey.PageUp, Keys.PageUp},
            {ConsoleKey.PageDown, Keys.PageDown},
            {ConsoleKey.OemComma, Keys.Comma},
            {(ConsoleKey)0xA1, Keys.Question},
            {ConsoleKey.Tab, Keys.Tab},
            {ConsoleKey.LeftWindows, Keys.LWin},
            {ConsoleKey.RightWindows, Keys.RWin},
        };

        KeyMappingReadOnly = new(KeyMapping);
    }

    public Keys? MapKey(ConsoleKey key)
    {
        if (!KeyMapping.TryGetValue(key, out var mappedKey)) return null;
        return mappedKey;
    }
}