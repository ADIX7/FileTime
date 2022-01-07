namespace FileTime.Core.Models
{
    public interface IElement : IItem
    {
        bool IsSpecial { get; }
        string GetPrimaryAttributeText();
    }
}