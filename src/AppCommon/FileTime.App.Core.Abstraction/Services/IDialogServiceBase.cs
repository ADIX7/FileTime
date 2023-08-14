using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;
using ObservableComputations;

namespace FileTime.App.Core.Services;

public interface IDialogServiceBase : IUserCommunicationService
{
    ScalarComputing<ReadInputsViewModel?> ReadInput { get; }
    ScalarComputing<MessageBoxViewModel?> LastMessageBox { get; }
}