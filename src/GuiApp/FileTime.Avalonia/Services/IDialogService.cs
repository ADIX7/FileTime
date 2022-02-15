using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileTime.Avalonia.Misc;
using FileTime.Core.Interactions;

namespace FileTime.Avalonia.Services
{
    public interface IDialogService
    {
        void CancelInputs();
        void CancelMessageBox();
        void ClearInputs();
        Task ProcessInputs();
        void ProcessMessageBox();
        void ReadInputs(List<InputElement> inputs, Action<List<InputElementWrapper>> inputHandler);
        void ReadInputs(List<InputElement> inputs, Func<List<InputElementWrapper>, Task> inputHandler);
        Task<string?[]> ReadInputs(IEnumerable<InputElement> fields);
        void ShowMessageBox(string text, Func<Task> inputHandler);
        void ShowToastMessage(string text);
    }
}