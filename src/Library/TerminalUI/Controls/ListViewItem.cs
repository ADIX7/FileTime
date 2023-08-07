namespace TerminalUI.Controls;

public class ListViewItem<T> : View<T>
{
    public override void Render()
    {
        Console.WriteLine(DataContext?.ToString());
    }
}