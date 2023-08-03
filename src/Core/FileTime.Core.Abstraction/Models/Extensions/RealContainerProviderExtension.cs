namespace FileTime.Core.Models.Extensions;

public class RealContainerProviderExtension
{
    public RealContainerProviderExtension(Func<AbsolutePath> realContainer)
    {
        RealContainer = realContainer;
    }

    public Func<AbsolutePath> RealContainer { get; }
}