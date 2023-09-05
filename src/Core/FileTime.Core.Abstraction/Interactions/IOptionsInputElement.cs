namespace FileTime.Core.Interactions;

public interface IOptionsInputElement : IInputElement
{
    object? Value { get; set; }
    IReadOnlyCollection<IOptionElement> Options { get; }
}