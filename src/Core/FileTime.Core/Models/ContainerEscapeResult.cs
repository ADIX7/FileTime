namespace FileTime.Core.Models
{
    public class ContainerEscapeResult
    {
        public bool Handled { get; }
        public IContainer? NavigateTo { get; }

        public ContainerEscapeResult(bool handled)
        {
            Handled = handled;
        }

        public ContainerEscapeResult(IContainer navigateTo)
        {
            NavigateTo = navigateTo;
        }
    }
}