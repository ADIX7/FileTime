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

        public void PrintUI()
        {
            if (Tab != null)
            {
                PrintPrompt();
                PrintTabs();
            }
        }

        private void PrintTabs()
        {
            var previousColumnWidth = (int)Math.Floor(Console.WindowWidth * 0.15) - 1;
            var currentColumnWidth = (int)Math.Floor(Console.WindowWidth * 0.4) - 1;
            var nextColumnWidth = Console.WindowWidth - currentColumnWidth - previousColumnWidth - 2;
            var currentVirtualContainer = Tab!.CurrentLocation as VirtualContainer;

            if (Tab.CurrentLocation.GetParent() is var parentContainer && parentContainer is not null)
            {
                parentContainer.Refresh();

                PrintColumn(
                    currentVirtualContainer != null
                        ? currentVirtualContainer.CloneVirtualChainFor(parentContainer, v => v.IsTransitive)
                        : parentContainer,
                    currentVirtualContainer != null
                        ? currentVirtualContainer.GetRealContainer()
                        : Tab.CurrentLocation,
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

            Tab.CurrentLocation.Refresh();

            CheckAndSetCurrentDisplayStartY();
            PrintColumn(
                Tab.CurrentLocation,
                Tab.CurrentSelectedItem,
                PrintMode.Current,
                previousColumnWidth + 1,
                _contentPaddingTop,
                currentColumnWidth,
                _contentRowCount);

            if (Tab.CurrentSelectedItem is IContainer selectedContainer)
            {
                selectedContainer.Refresh();

                selectedContainer = currentVirtualContainer != null
                    ? currentVirtualContainer.CloneVirtualChainFor(selectedContainer, v => v.IsTransitive)
                    : selectedContainer;

                PrintColumn(
                    selectedContainer,
                    selectedContainer.Items.Count > 0 ? selectedContainer.Items[0] : null,
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

        private void PrintPrompt()
        {
            Console.SetCursorPosition(0, 0);
            _coloredRenderer.ResetColor();
            _coloredRenderer.ForegroundColor = _appStyle.AccentForeground;
            _coloredRenderer.Write(Environment.UserName + "@" + Environment.MachineName);

            _coloredRenderer.ResetColor();
            _coloredRenderer.Write(' ');

            _coloredRenderer.ForegroundColor = _appStyle.ContainerForeground;
            var path = Tab!.CurrentLocation.FullName + "/";
            _coloredRenderer.Write(path);

            if (Tab.CurrentSelectedItem?.Name != null)
            {
                _coloredRenderer.ResetColor();
                _coloredRenderer.Write($"{{0,-{300 - path.Length}}}", Tab.CurrentSelectedItem.Name);
            }
        }

        private void PrintColumn(IContainer currentContainer, IItem? currentItem, PrintMode printMode, int startX, int startY, int elementWidth, int availableRows)
        {
            var allItem = currentContainer.Containers.Cast<IItem>().Concat(currentContainer.Elements).ToList();
            var printedItemsCount = 0;
            var currentY = 0;
            if (allItem.Count > 0)
            {
                var currentIndex = allItem.FindIndex(i => i == currentItem);

                var skipElements = printMode switch
                {
                    PrintMode.Previous => (currentIndex - (availableRows / 2)).Map(r => r < 0 ? 0 : r),
                    PrintMode.Current => _currentDisplayStartY,
                    PrintMode.Next => 0,
                    _ => 0
                };

                var maxTextWidth = elementWidth - ITEMPADDINGLEFT - ITEMPADDINGRIGHT;

                var itemsToPrint = currentContainer.Items.Skip(skipElements).Take(availableRows).ToList();
                printedItemsCount = itemsToPrint.Count;
                foreach (var item in itemsToPrint)
                {
                    var namePart = item.Name.Length > maxTextWidth
                        ? string.Concat(item.Name.AsSpan(0, maxTextWidth - 1), "~")
                        : item.Name;

                    var attributePart = "";

                    var container = item as IContainer;
                    var element = item as IElement;

                    attributePart = container != null ? "" + container.Items.Count : element!.GetPrimaryAttributeText();

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

                    if (item == currentItem)
                    {
                        (backgroundColor, foregroundColor) = (foregroundColor, backgroundColor);
                    }

                    _coloredRenderer.BackgroundColor = backgroundColor;
                    _coloredRenderer.ForegroundColor = foregroundColor;

                    var text = string.Format($"{{0,-{elementWidth}}}", _paddingLeft + (isSelected ? " " : "") + namePart + _paddingRight);
                    text = string.Concat(text.AsSpan(0, text.Length - attributePart.Length - 1), " ", attributePart);

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

        private void CheckAndSetCurrentDisplayStartY()
        {
            const int padding = 5;

            while (Tab.CurrentSelectedIndex < _currentDisplayStartY + padding
                    && _currentDisplayStartY > 0)
            {
                _currentDisplayStartY--;
            }

            while (Tab.CurrentSelectedIndex > _currentDisplayStartY + _contentRowCount - padding
                    && _currentDisplayStartY < Tab.CurrentLocation.Items.Count - _contentRowCount)
            {
                _currentDisplayStartY++;
            }
        }
    }
}