using Terminal.Gui;

namespace FileTime.ConsoleUI.App.Extensions;

public static class UiExtensions
{
    public static void Update(this ListView listView)
    {
        listView.EnsureSelectedItemVisible();
        listView.SetNeedsDisplay();
    }
}