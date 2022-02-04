namespace FileTime.Core.Models
{
    public interface IElement : IItem
    {
        bool IsSpecial { get; }
        string GetPrimaryAttributeText();
        Task<string> GetContent(CancellationToken token = default);
        Task<long> GetElementSize(CancellationToken token = default);
    }
}