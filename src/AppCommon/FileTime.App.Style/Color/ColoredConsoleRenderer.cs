namespace FileTime.ConsoleUI.App.UI.Color
{
    public class ColoredConsoleRenderer : IColoredConsoleRenderer
    {
        private readonly IStyles _styles;

        public IConsoleColor? BackgroundColor { get; set; }
        public IConsoleColor? ForegroundColor { get; set; }

        public ColoredConsoleRenderer(IStyles styles)
        {
            _styles = styles;
            BackgroundColor = _styles.DefaultBackground;
            ForegroundColor = _styles.DefaultForeground;
        }

        public void Write(string text) => DoWrite(text);
        public void Write(char c) => DoWrite(c.ToString());
        public void Write(string format, params object[] param) => DoWrite(string.Format(format, param));

        private void DoWrite(string text)
        {
            if (BackgroundColor is AnsiColor ansiBackground && ForegroundColor is AnsiColor ansiForeground)
            {
                var formatting = "\u001b[0m";

                if (ansiBackground.Color != null)
                {
                    formatting += "\u001b[48;" + ansiBackground.ToString();
                }

                if (ansiForeground.Color != null)
                {
                    formatting += "\u001b[38;" + ansiForeground.ToString();
                }

                Console.Write(formatting + text);
            }
            else if (BackgroundColor is BasicColor basicBackground && ForegroundColor is BasicColor basicForeground)
            {
                Console.BackgroundColor = basicBackground.Color ?? Console.BackgroundColor;
                Console.ForegroundColor = basicForeground.Color ?? Console.ForegroundColor;

                Console.Write(text);
            }
            else if (BackgroundColor == null && ForegroundColor == null)
            {
                Console.Write(text);
            }
            else if (BackgroundColor == null || ForegroundColor == null)
            {
                throw new Exception($"Either both of {nameof(BackgroundColor)} and {nameof(ForegroundColor)} must be null or neither of them.");
            }
            else if (BackgroundColor.GetType() != ForegroundColor.GetType())
            {
                throw new Exception($"Type of {nameof(BackgroundColor)} ({BackgroundColor.GetType()}) and {nameof(ForegroundColor)} ({ForegroundColor.GetType()}) must be the same.");
            }
            else
            {
                throw new Exception($"Unsupported color type: {BackgroundColor.GetType()}");
            }
        }

        public void ResetColor()
        {
            BackgroundColor = _styles.DefaultBackground;
            ForegroundColor = _styles.DefaultForeground;

            Console.ResetColor();
        }

        public void Clear()
        {
            try
            {
                Console.SetCursorPosition(0, 0);
                Write(new string(' ', Console.WindowHeight * Console.WindowWidth));
            }
            catch { }
        }
    }
}