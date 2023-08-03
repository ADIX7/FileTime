namespace FileTime.Core.Models.Extensions;

public class EscHandlerContainerExtension
{
    private readonly Func<Task<ContainerEscapeResult>> _handleEsc;

    public EscHandlerContainerExtension(Func<Task<ContainerEscapeResult>> handleEsc)
    {
        _handleEsc = handleEsc;
    }
    public async Task<ContainerEscapeResult> HandleEsc() => await _handleEsc();
}