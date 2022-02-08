using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Misc;
using FileTime.Core.Interactions;

namespace FileTime.Avalonia.Services
{
    public class DialogService
    {
        private readonly AppState _appState;

        private Func<List<InputElementWrapper>, Task>? _inputHandler;

        public DialogService(AppState appState)
        {
            _appState = appState;
        }
        public void ReadInputs(List<InputElement> inputs, Action<List<InputElementWrapper>> inputHandler) =>
            ReadInputs(inputs, (inputs) => { inputHandler(inputs); return Task.CompletedTask; });

        public void ReadInputs(List<InputElement> inputs, Func<List<InputElementWrapper>, Task> inputHandler)
        {
            _appState.Inputs = inputs.ConvertAll(i => new InputElementWrapper(i, i.DefaultValue));
            _inputHandler = inputHandler;
        }

        public async Task<string?[]> ReadInputs(IEnumerable<InputElement> fields)
        {
            var waiting = true;
            var result = Array.Empty<string>();
            ReadInputs(fields.ToList(), (inputs) =>
            {
                if (inputs != null)
                {
                    result = inputs.Select(i => i.Value).ToArray();
                }
                waiting = false;
            });

            while (waiting) await Task.Delay(100);

            return result;
        }

        public void ClearInputs()
        {
            _appState.Inputs = null;
            _inputHandler = null;
        }

        public async Task ProcessInputs()
        {
            try
            {
                if (_inputHandler != null)
                {
                    await _inputHandler.Invoke(_appState.Inputs);
                }
            }
            catch { }

            ClearInputs();
        }

        public void CancelInputs()
        {
            ClearInputs();
        }

        public void ShowMessageBox(string text, Func<Task> inputHandler)
        {
            _appState.MessageBoxText = text;
            _inputHandler = async (_) => await inputHandler();
        }

        public void ProcessMessageBox()
        {
            _inputHandler?.Invoke(null!);

            _appState.MessageBoxText = null;
            _inputHandler = null;
        }

        public void CancelMessageBox()
        {
            _appState.MessageBoxText = null;
            _inputHandler = null;
        }

        public void ShowToastMessage(string text)
        {
            _appState.PopupTexts.Add(text);

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                await Dispatcher.UIThread.InvokeAsync(() => _appState.PopupTexts.Remove(text));
            });
        }
    }
}