namespace FileTime.Core.Models
{
    public interface IFile : IElement
    {
        string Attributes { get; }
        DateTime CreatedAt { get; }
    }
}