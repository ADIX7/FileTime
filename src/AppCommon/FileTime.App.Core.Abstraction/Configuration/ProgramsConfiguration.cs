namespace FileTime.App.Core.Configuration;

public class ProgramsConfiguration
{
    public List<ProgramConfiguration> DefaultEditorPrograms { get; set; } = new();
    public List<ProgramConfiguration> EditorPrograms { get; set; } = new();
}