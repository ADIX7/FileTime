using System.Collections.Generic;

namespace FileTime.GuiApp.Configuration;

public class ProgramsConfiguration
{
    public List<ProgramConfiguration> DefaultEditorPrograms { get; set; } = new();
    public List<ProgramConfiguration> EditorPrograms { get; set; } = new();
}