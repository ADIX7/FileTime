using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FileTime.Core.Interactions;

public abstract class InputElementBase : IInputElement
{
    private readonly BehaviorSubject<bool> _isValid = new(true);
    public InputType Type { get; }
    public string Label { get; }
    public IObservable<bool> IsValid { get; }

    protected InputElementBase(string label, InputType type)
    {
        Label = label;
        Type = type;
        IsValid = _isValid.AsObservable();
    }

    public void SetValid() => _isValid.OnNext(true);

    public void SetInvalid() => _isValid.OnNext(false);
}