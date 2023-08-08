﻿using TerminalUI.Traits;

namespace TerminalUI.Controls;

public class ListViewItem<T> : ContentView<T>
{
    protected override void DefaultRenderer()
    {
        if (ContentRendererMethod is null)
        {
            throw new NullReferenceException(
                nameof(ContentRendererMethod)
                + " is null, cannot render content of "
                + Content?.GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        ContentRendererMethod();
    }
}