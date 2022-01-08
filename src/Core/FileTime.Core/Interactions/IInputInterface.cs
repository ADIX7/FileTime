namespace FileTime.Core.Interactions
{
    public interface IInputInterface
    {
        string?[] ReadInputs(IEnumerable<InputElement> fields);
    }
}