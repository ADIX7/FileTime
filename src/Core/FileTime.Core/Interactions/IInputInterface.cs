namespace FileTime.Core.Interactions
{
    public interface IInputInterface
    {
        Task<string?[]> ReadInputs(IEnumerable<InputElement> fields);
    }
}