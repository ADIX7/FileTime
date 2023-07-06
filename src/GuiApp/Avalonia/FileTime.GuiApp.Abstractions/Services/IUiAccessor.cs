﻿using Avalonia.Controls;

namespace FileTime.GuiApp.Services;

public interface IUiAccessor
{
    public TopLevel? GetTopLevel();
    Task InvokeOnUIThread(Func<Task> func);
    Task<T> InvokeOnUIThread<T>(Func<Task<T>> func);
}