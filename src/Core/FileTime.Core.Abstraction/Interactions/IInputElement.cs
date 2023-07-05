namespace FileTime.Core.Interactions;

public interface IInputElement
{
    InputType Type { get; }
    string Label { get; }
    IObservable<bool> IsValid { get; }
    void SetValid();
    void SetInvalid();
}