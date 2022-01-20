using FileTime.App.Core.Tab;
using FileTime.ConsoleUI.App.UI.Color;
using FileTime.ConsoleUI.UI.App;
using FileTime.Core.Components;
using FileTime.Core.Extensions;
using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App.UI
{
    public class Render
    {
        private const int _contentPaddingTop = 1;
        private const int _contentPaddingBottom = 2;
        private readonly int _contentRowCount;
        private readonly IStyles _appStyle;
        private readonly IColoredConsoleRenderer _coloredRenderer;

        private int _currentDisplayStartY;

        private const int ITEMPADDINGLEFT = 1;
        private const int ITEMPADDINGRIGHT = 2;

        private readonly string _paddingLeft;
        private readonly string _paddingRight;

        public Tab Tab { get; private set; }
        public TabState TabState { get; private set; }

        public int PageSize => Console.WindowHeight - _contentPaddingTop - _contentPaddingBottom;
        public Render(IColoredConsoleRenderer coloredRenderer, IStyles appStyle)
        {
            _coloredRenderer = coloredRenderer;
            _appStyle = appStyle;

            _paddingLeft = new string(' ', ITEMPADDINGLEFT);
            _paddingRight = new string(' ', ITEMPADDINGRIGHT);
            _contentRowCount = Console.WindowHeight - _contentPaddingTop - _contentPaddingBottom;
        }

        public void Init(Tab pane, TabState paneState)
        {
            if (pane == null) throw new Exception($"{nameof(pane)} can not be null");
            if (paneState == null) throw new Exception($"{nameof(paneState)} can not be null");

            Tab = pane;
            Tab.CurrentLocationChanged += (o, e) => _currentDisplayStartY = 0;

            TabState = paneState;
        }

        public async Task PrintUI(CancellationToken token = default)
        {
            if (Tab != null)
            {
                await PrintPrompt(token);
                await PrintTabs(token);
            }
        }

        private async Task PrintTabs(CancellationToken token = default)
        {
            var previousColumnWidth = (int)Math.Floor(Console.WindowWidth * 0.15) - 1;
            var currentColumnWidth = (int)Math.Floor(Console.WindowWidth * 0.4) - 1;
            var nextColumnWidth = Console.WindowWidth - currentColumnWidth - previousColumnWidth - 2;

            var currentLocation = (await Tab!.GetCurrentLocation())!;
            var currentSelectedItem = (await Tab!.GetCurrentSelectedItem())!;

            var currentVirtualContainer = currentLocation as VirtualContainer;

            if (currentLocation.GetParent() is var parentContainer && parentContainer is not null)
            {
                await parentContainer.Refresh();

                await PrintColumn(
                    currentVirtualContainer != null
                        ? currentVirtualContainer.CloneVirtualChainFor(parentContainer, v => v.IsTransitive)
                        : parentContainer,
                    currentVirtualContainer != null
                        ? currentVirtualContainer.GetRealContainer()
                        : currentLocation,
                    PrintMode.Previous,
                    0,
                    _contentPaddingTop,
                    previousColumnWidth,
                    _contentRowCount);
            }
            else
            {
                CleanColumn(
                    0,
                    _contentPaddingTop,
                    previousColumnWidth,
                    _contentRowCount);
            }

            if (token.IsCancellationRequested) return;

            await currentLocation.Refresh();

            await CheckAndSetCurrentDisplayStartY();
            await PrintColumn(
                currentLocation,
                currentSelectedItem,
                PrintMode.Current,
                previousColumnWidth + 1,
                _contentPaddingTop,
                currentColumnWidth,
                _contentRowCount);

            if (token.IsCancellationRequested) return;

            if (currentSelectedItem is IContainer selectedContainer)
            {
                await selectedContainer.Refresh();

                selectedContainer = currentVirtualContainer != null
                    ? currentVirtualContainer.CloneVirtualChainFor(selectedContainer, v => v.IsTransitive)
                    : selectedContainer;

                var selectedContainerItems = (await selectedContainer.GetItems())!;

                await PrintColumn(
                    selectedContainer,
                    selectedContainerItems.Count > 0 ? selectedContainerItems[0] : null,
                    PrintMode.Next,
                    previousColumnWidth + currentColumnWidth + 2,
                    _contentPaddingTop,
                    nextColumnWidth,
                    _contentRowCount);
            }
            else
            {
                CleanColumn(
                    previousColumnWidth + currentColumnWidth + 2,
                    _contentPaddingTop,
                    nextColumnWidth,
                    _contentRowCount);
            }
        }

        private async Task PrintPrompt(CancellationToken token = default)
        {
            var currentLocation = await Tab!.GetCurrentLocation();
            var currentSelectedItem = await Tab!.GetCurrentSelectedItem();

            Console.SetCursorPosition(0, 0);
            _coloredRenderer.ResetColor();
            _coloredRenderer.ForegroundColor = _appStyle.AccentForeground;
            _coloredRenderer.Write(Environment.UserName + "@" + Environment.MachineName);

            _coloredRenderer.ResetColor();
            _coloredRenderer.Write(' ');

            _coloredRenderer.ForegroundColor = _appStyle.ContainerForeground;
            var path = currentLocation.FullName + "/";
            _coloredRenderer.Write(path);

            if (currentSelectedItem?.Name != null)
            {
                _coloredRenderer.ResetColor();
                _coloredRenderer.Write($"{{0,-{300 - path.Length}}}", currentSelectedItem.Name);
            }
        }

        private async Task PrintColumn(IContainer currentContainer, IItem? currentItem, PrintMode printMode, int startX, int startY, int elementWidth, int availableRows, CancellationToken token = default)
        {
            var allItem = (await currentContainer.GetItems())!.ToList();
            var printedItemsCount = 0;
            var currentY = 0;
            if (allItem.Count > 0)
            {
                var currentIndex = allItem.FindIndex(i => i.Name == currentItem?.Name);

                var skipElements = printMode switch
                {
                    PrintMode.Previous => (currentIndex - (availableRows / 2)).Map(r => r < 0 ? 0 : r),
                    PrintMode.Current => _currentDisplayStartY,
                    PrintMode.Next => 0,
                    _ => 0
                };

                var maxTextWidth = elementWidth - ITEMPADDINGLEFT - ITEMPADDINGRIGHT;

                var itemsToPrint = (await currentContainer.GetItems())!.Skip(skipElements).Take(availableRows).ToList();
                printedItemsCount = itemsToPrint.Count;
                foreach (var item in itemsToPrint)
                {
                    var namePart = item.Name.Length > maxTextWidth
                        ? string.Concat(item.Name.AsSpan(0, maxTextWidth - 1), "~")
                        : item.Name;

                    var attributePart = "";

                    var container = item as IContainer;
                    var element = item as IElement;

                    attributePart = container != null ? "" + (await container.GetItems())!.Count : element!.GetPrimaryAttributeText();

                    IConsoleColor? backgroundColor = null;
                    IConsoleColor? foregroundColor = null;

                    if (container != null)
                    {
                        backgroundColor = _appStyle.ContainerBackground;
                        foregroundColor = _appStyle.ContainerForeground;
                    }
                    else if (element != null)
                    {
                        if (element.IsSpecial)
                        {
                            backgroundColor = _appStyle.ElementSpecialBackground;
                            foregroundColor = _appStyle.ElementSpecialForeground;
                        }
                        else
                        {
                            backgroundColor = _appStyle.ElementBackground;
                            foregroundColor = _appStyle.ElementForeground;
                        }
                    }

                    var isSelected = TabState.ContainsSelectedItem(item.Provider, currentContainer, item.FullName!);
                    if (isSelected)
                    {
                        backgroundColor = _appStyle.SelectedItemBackground;
                        foregroundColor = _appStyle.SelectedItemForeground;
                    }

                    if (item.Name == currentItem?.Name)
                    {
                        (backgroundColor, foregroundColor) = (foregroundColor, backgroundColor);
                    }

                    _coloredRenderer.BackgroundColor = backgroundColor;
                    _coloredRenderer.ForegroundColor = foregroundColor;

                    var text = string.Format($"{{0,-{elementWidth}}}", _paddingLeft + (isSelected ? " " : "") + namePart + _paddingRight);
                    text = string.Concat(text.AsSpan(0, text.Length - attributePart.Length - 1), " ", attributePart);

                    if (token.IsCancellationRequested) return;
                    Console.SetCursorPosition(startX, startY + currentY++);
                    _coloredRenderer.Write(text);

                    _coloredRenderer.ResetColor();
                }
            }
            else
            {
                _coloredRenderer.BackgroundColor = _appStyle.ErrorColor;
                _coloredRenderer.ForegroundColor = _appStyle.ErrorInverseColor;
                Console.SetCursorPosition(startX, startY + currentY++);

                _coloredRenderer.Write($"{{0,-{elementWidth}}}", _paddingLeft + "<empty>" + _paddingRight);
                printedItemsCount++;
            }

            var padding = new string(' ', elementWidth);
            _coloredRenderer.ResetColor();
            for (var i = 0; i < availableRows - printedItemsCount + 1; i++)
            {
                Console.SetCursorPosition(startX, startY + currentY++);
                _coloredRenderer.Write(padding);
            }
        }

        private void CleanColumn(int startX, int startY, int elementWidth, int availableRows)
        {
            _coloredRenderer.ResetColor();

            var currentY = 0;
            var placeholder = new string(' ', elementWidth);
            for (var i = 0; i < availableRows; i++)
            {
                Console.SetCursorPosition(startX, startY + currentY++);
                _coloredRenderer.Write(placeholder);
            }
        }

        private async Task CheckAndSetCurrentDisplayStartY()
        {
            const int padding = 5;

            while (Tab.CurrentSelectedIndex < _currentDisplayStartY + padding
                    && _currentDisplayStartY > 0)
            {
                _currentDisplayStartY--;
            }

            while (Tab.CurrentSelectedIndex > _currentDisplayStartY + _contentRowCount - padding
                    && _currentDisplayStartY < (await (await Tab.GetCurrentLocation()).GetItems())!.Count - _contentRowCount)
            {
                _currentDisplayStartY++;
            }
        }
    }
}