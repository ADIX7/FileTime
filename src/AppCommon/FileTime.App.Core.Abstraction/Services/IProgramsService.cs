using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public interface IProgramsService
{
    ProgramConfiguration? GetEditorProgram(bool getNext = false);
    void ResetLastGoodEditor();
}