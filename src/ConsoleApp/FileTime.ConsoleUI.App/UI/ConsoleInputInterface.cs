using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Interactions;

namespace FileTime.ConsoleUI.App.UI
{
    public class ConsoleInputInterface : IInputInterface
    {
        private readonly Application _application;
        private readonly IColoredConsoleRenderer _coloredConsoleRenderer;
        private readonly ConsoleReader _consoleReader;

        public ConsoleInputInterface(Application application, IColoredConsoleRenderer coloredConsoleRenderer, ConsoleReader consoleReader)
        {
            _application = application;
            _coloredConsoleRenderer = coloredConsoleRenderer;
            _consoleReader = consoleReader;
        }
        public string?[] ReadInputs(IEnumerable<InputElement> fields)
        {
            var results = new List<string?>();

            _coloredConsoleRenderer.ResetColor();

            foreach (var input in fields)
            {
                _application.MoveToIOLine();
                _coloredConsoleRenderer.Write(input.Text + ": ");

                results.Add(_consoleReader.ReadText(placeHolder: input.InputType == InputType.Password ? '*' : null));
            }

            return results.ToArray();
        }
    }
}