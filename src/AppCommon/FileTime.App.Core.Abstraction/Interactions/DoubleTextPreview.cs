﻿using System.Reactive.Subjects;
using FileTime.Core.Interactions;
using FileTime.Core.Models;

namespace FileTime.App.Core.Interactions;

public class DoubleTextPreview : IPreviewElement
{
    public IObservable<List<ItemNamePart>> Text1 { get; init; } = new BehaviorSubject<List<ItemNamePart>>(new());
    public IObservable<List<ItemNamePart>> Text2 { get; init; } = new BehaviorSubject<List<ItemNamePart>>(new());
    
    public PreviewType PreviewType => PreviewType.DoubleTextList;
    object IPreviewElement.PreviewType => PreviewType;
}