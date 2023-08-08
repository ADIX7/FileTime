using PropertyChanged.SourceGenerator;
using TerminalUI.Extensions;

namespace TerminalUI.Controls;

public partial class TextBlock<T> : View<T>
{
    [Notify] private string? _text = string.Empty;

    public TextBlock()
    {
        this.Bind(
            this,
            dc => dc == null ? string.Empty : dc.ToString(),
            tb => tb.Text
        );

        RerenderProperties.Add(nameof(Text));
    }

    protected override void DefaultRenderer()
        => Console.Write(Text);
}