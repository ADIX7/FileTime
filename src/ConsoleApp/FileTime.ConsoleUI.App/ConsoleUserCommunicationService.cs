using FileTime.Core.Interactions;

namespace FileTime.ConsoleUI.App;

public class ConsoleUserCommunicationService : IUserCommunicationService
{
    public Task<bool> ReadInputs(params IInputElement[] fields) => throw new NotImplementedException();

    public Task<bool> ReadInputs(IInputElement field, IEnumerable<IPreviewElement>? previews = null) => throw new NotImplementedException();

    public Task<bool> ReadInputs(IEnumerable<IInputElement> fields, IEnumerable<IPreviewElement>? previews = null) => throw new NotImplementedException();

    public void ShowToastMessage(string text) => throw new NotImplementedException();

    public Task<MessageBoxResult> ShowMessageBox(string text, bool showCancel = true, string? okText = null, string? cancelText = null) => throw new NotImplementedException();
}