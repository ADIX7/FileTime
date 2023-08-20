using FileTime.App.Core.Services;
using FileTime.App.Core.Services.UserCommandHandler;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.ConsoleUI.App.Preview;

namespace FileTime.ConsoleUI.App.UserCommand;

public class ConsoleUserCommandHandler : AggregatedUserCommandHandler
{
    private static readonly ItemPreviewType[] ElementPreviewOrder = {ItemPreviewType.Text, ItemPreviewType.Binary};

    private readonly IConsoleAppState _consoleAppState;
    private readonly IItemPreviewService _itemPreviewService;

    public ConsoleUserCommandHandler(
        IConsoleAppState consoleAppState,
        IItemPreviewService itemPreviewService
    )
    {
        _consoleAppState = consoleAppState;
        _itemPreviewService = itemPreviewService;
        AddCommandHandler(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<NextPreviewUserCommand>(NextPreview),
            new TypeUserCommandHandler<PreviousPreviewUserCommand>(PreviousPreview),
        });
    }

    private Task NextPreview()
    {
        if (_itemPreviewService.ItemPreview.Value is not IElementPreviewViewModel) return Task.CompletedTask;

        var previewOrder = ElementPreviewOrder;
        if (previewOrder.Length < 2) return Task.CompletedTask;

        if (_consoleAppState.PreviewType == null)
        {
            _consoleAppState.PreviewType = previewOrder.Length > 1 ? previewOrder[1] : null;
            return Task.CompletedTask;
        }

        var currentPreviewType = _consoleAppState.PreviewType.Value;
        int i;
        for (i = 0; i < previewOrder.Length; i++)
        {
            if (previewOrder[i] == currentPreviewType) break;
        }

        i++;

        _consoleAppState.PreviewType = i >= previewOrder.Length ? previewOrder[0] : previewOrder[i];
        return Task.CompletedTask;
    }

    private Task PreviousPreview()
    {
        if (_itemPreviewService.ItemPreview.Value is not IElementPreviewViewModel) return Task.CompletedTask;

        var previewOrder = ElementPreviewOrder;
        if (previewOrder.Length < 2) return Task.CompletedTask;

        if (_consoleAppState.PreviewType == null)
        {
            _consoleAppState.PreviewType = previewOrder.Length > 1 ? previewOrder[^1] : null;
            return Task.CompletedTask;
        }

        var currentPreviewType = _consoleAppState.PreviewType.Value;
        int i;
        for (i = previewOrder.Length - 1; i > -1; i--)
        {
            if (previewOrder[i] == currentPreviewType) break;
        }

        i--;

        _consoleAppState.PreviewType = i > -1 ? previewOrder[^1] : previewOrder[i];
        return Task.CompletedTask;
    }
}