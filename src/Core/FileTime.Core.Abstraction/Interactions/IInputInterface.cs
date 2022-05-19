namespace FileTime.Core.Interactions;

public interface IInputInterface
{
    Task<bool> ReadInputs(IEnumerable<IInputElement> fields);
}