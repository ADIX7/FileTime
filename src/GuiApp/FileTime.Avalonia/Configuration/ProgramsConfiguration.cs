using System.Collections.Generic;

namespace FileTime.Avalonia.Configuration
{
    public class ProgramsConfiguration
    {
        public List<ProgramConfiguration> DefaultEditorPrograms { get; set; } = new();
        public List<ProgramConfiguration> EditorPrograms { get; set; } = new();
    }
}