﻿using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Preview;
using PropertyChanged.SourceGenerator;

namespace FileTime.ConsoleUI.App;

public partial class ConsoleAppState : AppStateBase, IConsoleAppState
{
    [Notify] private string? _errorText;
    //TODO: make it thread safe
    public ObservableCollection<string> PopupTexts { get; } = new();
    
    [Notify] private ItemPreviewType? _previewType = ItemPreviewType.Binary;
}