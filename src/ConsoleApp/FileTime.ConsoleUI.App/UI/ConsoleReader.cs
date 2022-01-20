using FileTime.ConsoleUI.App.UI.Color;

namespace FileTime.ConsoleUI.App.UI
{
    public class ConsoleReader
    {
        private readonly IColoredConsoleRenderer _coloredConsoleRenderer;

        public ConsoleReader(IColoredConsoleRenderer coloredConsoleRenderer)
        {
            _coloredConsoleRenderer = coloredConsoleRenderer;
        }
        public async Task<string?> ReadText(int? maxLength = null, Func<string, Task>? validator = null, char? placeHolder = null)
        {
            var cursorVisible = false;
            try
            {
                cursorVisible = Console.CursorVisible;
            }
            catch { }

            Console.CursorVisible = true;

            var currentConsoleLeft = Console.CursorLeft;
            var currentConsoleTop = Console.CursorTop;

            maxLength ??= Console.WindowWidth - currentConsoleLeft;

            var input = "";
            var position = 0;

            _coloredConsoleRenderer.Write($"{{0,-{maxLength}}}", input);

            var key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Escape)
                {
                    input = null;
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (position != 0)
                    {
                        input = input.Length > 0
                            ? input[..(position - 1)] + input[position..]
                            : input;

                        position--;
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (position > 0)
                        position--;
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (position < input.Length)
                        position++;
                }
                else if (key.KeyChar != '\0')
                {
                    var newInput = input[..position] + key.KeyChar;

                    if (position < input.Length)
                    {
                        newInput += input[position..];
                    }

                    input = newInput;

                    position++;
                }
                await validator?.Invoke(input);

                Console.SetCursorPosition(currentConsoleLeft, currentConsoleTop);
                _coloredConsoleRenderer.Write($"{{0,-{maxLength}}}", placeHolder == null ? input : new string((char)placeHolder, input.Length));

                Console.SetCursorPosition(currentConsoleLeft + position, currentConsoleTop);
                key = Console.ReadKey(true);
            }

            Console.CursorVisible = cursorVisible;
            return input;
        }
    }
}