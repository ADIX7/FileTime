namespace FileTime.Core.Interactions;

public interface IUserCommunicationService
{
    Task<bool> ReadInputs(params IInputElement[] fields);
    Task<bool> ReadInputs(IInputElement field, IEnumerable<IPreviewElement>? previews = null);
    Task<bool> ReadInputs(IEnumerable<IInputElement> fields, IEnumerable<IPreviewElement>? previews = null);
    void ShowToastMessage(string text);
    Task<MessageBoxResult> ShowMessageBox(
        string text,
        bool showCancel = true,
        string? okText = null,
        string? cancelText = null
    );
}