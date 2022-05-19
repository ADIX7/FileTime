namespace FileTime.Core.Interactions;

public interface IOptionsInputElement : IInputElement
{
    object Value { get; set; }
    IEnumerable<IOptionElement> Options { get; }
}