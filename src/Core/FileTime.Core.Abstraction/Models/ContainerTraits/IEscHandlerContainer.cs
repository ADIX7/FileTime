namespace FileTime.Core.Models.ContainerTraits;

public interface IEscHandlerContainer
{
    Task<ContainerEscapeResult> HandleEsc();
}