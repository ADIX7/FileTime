namespace FileTime.Core.Interactions;

public interface IUserCommunicationService
{
    Task<bool> ReadInputs(params IInputElement[] fields);
    void ShowToastMessage(string text);
}