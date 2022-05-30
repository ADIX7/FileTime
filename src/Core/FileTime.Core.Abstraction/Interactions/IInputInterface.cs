namespace FileTime.Core.Interactions;

public interface IInputInterface
{
    Task<bool> ReadInputs(params IInputElement[] fields);
}