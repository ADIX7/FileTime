using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.ConsoleUI.App.Styling;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class ItemPreviews
{
    private readonly ITheme _theme;

    public ItemPreviews(ITheme theme)
    {
        _theme = theme;
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
                        dc => (IElementPreviewViewModel)dc.ItemPreviewService.ItemPreview.Value
                    )
            }
        };

        return view;
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
                    v => v,
                    fallbackValue: false)),
                new TextBlock<IElementPreviewViewModel>
                {
                    TextAlignment = TextAlignment.Center,
                    Text = "Empty",
                }.Setup(t => t.Bind(
                    t,
                    dc => dc.Mode == ItemPreviewMode.Empty,
                    t => t.IsVisible,
                    v => v,
                    fallbackValue: false)),
                new Grid<IElementPreviewViewModel>
                {
                    RowDefinitionsObject = "* Auto",
                    ChildInitializer =
                    {
                        new TextBlock<IElementPreviewViewModel>()
                            .Setup(t => t.Bind(
                                t,
                                dc => dc.TextContent,
                                t => t.Text,
                                fallbackValue: string.Empty)),
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
                    v => v,
                    fallbackValue: false)),
            }
        };

        view.Bind(
            view,
            dc => dc.Name == ElementPreviewViewModel.PreviewName,
            v => v.IsVisible,
            v => v
        );

        return view;
    }
}