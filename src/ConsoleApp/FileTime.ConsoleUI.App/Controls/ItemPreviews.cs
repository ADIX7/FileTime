using System.Collections.ObjectModel;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.Core.Helpers;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.ConsoleUI.App.Preview;
using FileTime.ConsoleUI.App.Styling;
using Humanizer.Bytes;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class ItemPreviews
{
    private readonly ITheme _theme;
    private readonly IConsoleAppState _appState;
    private readonly IColorProvider _colorProvider;
    private ItemsControl<ContainerPreview, ISizePreviewItem> _sizePreviews;

    public ItemPreviews(
        ITheme theme,
        IConsoleAppState appState,
        IColorProvider colorProvider
    )
    {
        _theme = theme;
        _appState = appState;
        _colorProvider = colorProvider;
    }

    public IView<IRootViewModel> View()
    {
        var view = new Grid<IRootViewModel>()
        {
            ChildInitializer =
            {
                new TextBlock<IRootViewModel>
                {
                    TextAlignment = TextAlignment.Center,
                    Text = "Empty",
                    Foreground = _theme.ErrorForegroundColor,
                }.Setup(t => t.Bind(
                    t,
                    dc => dc.AppState.SelectedTab.Value.SelectedsChildren.Value.Count == 0,
                    t => t.IsVisible,
                    fallbackValue: false)),
                ElementPreviews()
                    .WithDataContextBinding<IRootViewModel, IElementPreviewViewModel>(
                        dc => (IElementPreviewViewModel) dc.ItemPreviewService.ItemPreview.Value
                    ),
                SizeContainerPreview()
                    .WithDataContextBinding<IRootViewModel, ContainerPreview>(
                        dc => (ContainerPreview) dc.ItemPreviewService.ItemPreview.Value
                    )
            }
        };

        return view;
    }

    private IView<ContainerPreview> SizeContainerPreview()
    {
        var sizePreviews = new ItemsControl<ContainerPreview, ISizePreviewItem>
        {
            Orientation = Orientation.Horizontal,
            Margin = "0 0 0 1"
        };
        sizePreviews.Setup(c => c.Bind(
            c,
            dc => dc.TopItems,
            c => c.ItemsSource));

        _sizePreviews = sizePreviews;

        sizePreviews.ItemTemplate = SizeContainerItem;

        var root = new Grid<ContainerPreview>
        {
            RowDefinitionsObject = "Auto Auto",
            ChildInitializer =
            {
                sizePreviews,
                new ItemsControl<ContainerPreview, ISizePreviewItem>
                    {
                        Extensions = {new GridPositionExtension(0, 1)},
                        ItemTemplate = () =>
                        {
                            var root = new Grid<ISizePreviewItem>
                            {
                                ColumnDefinitionsObject = "5 11 *",
                                ChildInitializer =
                                {
                                    new Rectangle<ISizePreviewItem>
                                        {
                                            Height = 1,
                                            Width = 3
                                        }
                                        .Setup(r => r.Bind(
                                            r,
                                            dc => GenerateSizeBackground(_sizePreviews.DataContext.TopItems, dc),
                                            r => r.Fill)),
                                    new TextBlock<ISizePreviewItem>
                                        {
                                            Extensions = {new GridPositionExtension(1, 0)},
                                            TextAlignment = TextAlignment.Right,
                                            Margin = "0 0 1 0"
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc.Size.Value,
                                            t => t.Text,
                                            v => ByteSize.FromBytes(v).ToString())),
                                    new TextBlock<ISizePreviewItem>
                                        {
                                            Extensions = {new GridPositionExtension(2, 0)}
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc.Name,
                                            t => t.Text))
                                }
                            };

                            return root;
                        }
                    }
                    .Setup(c => c.Bind(
                        c,
                        dc => dc.TopItems,
                        c => c.ItemsSource))
            }
        };

        root.Bind(
            root,
            dc => dc.Name == ContainerPreview.PreviewName,
            r => r.IsVisible);

        return root;
    }

    private Grid<ISizePreviewItem> SizeContainerItem()
    {
        var root = new Grid<ISizePreviewItem>
        {
            ChildInitializer =
            {
                new Rectangle<ISizePreviewItem>
                    {
                        Height = 1
                    }
                    .Setup(r => r.Bind(
                        r,
                        dc => GenerateSizeBackground(_sizePreviews.DataContext.TopItems, dc),
                        r => r.Fill))
            }
        };

        root.Bind(
            root,
            dc => GetWidth(dc.Size.Value, _sizePreviews.DataContext.TopItems, _sizePreviews.ActualWidth),
            r => r.Width);

        return root;
    }

    private int? GetWidth(long sizeValue, ObservableCollection<ISizePreviewItem>? dataContextTopItems, int rootActualWidth)
        => dataContextTopItems is null
            ? 0
            : (int) Math.Floor((double) rootActualWidth * sizeValue / dataContextTopItems.Select(i => i.Size.Value).Sum());

    private IColor? GenerateSizeBackground(ObservableCollection<ISizePreviewItem> topItems, ISizePreviewItem? dc)
    {
        if (dc is null) return null;
        var (r, g, b) = SizePreviewItemHelper.GetItemColor(topItems, dc);

        return _colorProvider.FromRgb(new Rgb(r, g, b), ColorType.Background);
    }

    private IView<IElementPreviewViewModel> ElementPreviews()
    {
        var view = new Grid<IElementPreviewViewModel>
        {
            ChildInitializer =
            {
                new TextBlock<IElementPreviewViewModel>
                {
                    TextAlignment = TextAlignment.Center,
                    Text = "Don't know how to preview this item.",
                }.Setup(t => t.Bind(
                    t,
                    dc => dc.Mode == ItemPreviewMode.Unknown,
                    t => t.IsVisible,
                    fallbackValue: false)),
                new TextBlock<IElementPreviewViewModel>
                {
                    TextAlignment = TextAlignment.Center,
                    Text = "Empty",
                }.Setup(t => t.Bind(
                    t,
                    dc => dc.Mode == ItemPreviewMode.Empty,
                    t => t.IsVisible,
                    fallbackValue: false)),
                new Grid<IElementPreviewViewModel>
                {
                    RowDefinitionsObject = "* Auto",
                    ChildInitializer =
                    {
                        new TextBlock<IElementPreviewViewModel>()
                            .Setup(t =>
                            {
                                t.Bind(
                                    t,
                                    dc => dc.TextContent,
                                    t => t.Text,
                                    fallbackValue: string.Empty);

                                t.Bind(
                                    t,
                                    dc => _appState.PreviewType,
                                    t => t.IsVisible,
                                    v => v is null or ItemPreviewType.Text);
                            }),
                        new BinaryView<IElementPreviewViewModel>()
                            .Setup(b =>
                            {
                                b.Bind(
                                    b,
                                    dc => dc.BinaryContent,
                                    b => b.Data);

                                b.Bind(
                                    b,
                                    dc => _appState.PreviewType,
                                    t => t.IsVisible,
                                    v => v == ItemPreviewType.Binary);
                            }),
                        new TextBlock<IElementPreviewViewModel>
                        {
                            Margin = "0 1 0 0",
                            Extensions = {new GridPositionExtension(0, 1)}
                        }.Setup(t => t.Bind(
                            t,
                            dc => dc.TextEncoding,
                            t => t.Text,
                            v => $"Encoding: {v}"))
                    }
                }.Setup(t => t.Bind(
                    t,
                    dc => dc.Mode == ItemPreviewMode.Text,
                    t => t.IsVisible,
                    fallbackValue: false)),
            }
        };

        view.Bind(
            view,
            dc => dc.Name == ElementPreviewViewModel.PreviewName,
            v => v.IsVisible
        );

        return view;
    }
}