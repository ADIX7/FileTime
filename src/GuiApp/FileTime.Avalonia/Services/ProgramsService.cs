using System.Collections.Generic;
using FileTime.Avalonia.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.Avalonia.Services
{
    public class ProgramsService
    {
        private readonly List<ProgramConfiguration> _editorPrograms;
        private int lastGoodEditorProgramIndex;

        public ProgramsService(IOptions<ProgramsConfiguration> configuration)
        {
            var config = configuration.Value;
            _editorPrograms = config.EditorPrograms.Count == 0 ? config.DefaultEditorPrograms : config.EditorPrograms;
        }

        public ProgramConfiguration? GetEditorProgram(bool getNext = false)
        {
            if (getNext)
            {
                lastGoodEditorProgramIndex++;
            }
            if (lastGoodEditorProgramIndex < 0)
            {
                lastGoodEditorProgramIndex = 0;
            }

            if (_editorPrograms.Count <= lastGoodEditorProgramIndex)
            {
                ResetLastGoodEditor();
                return null;
            }
            return _editorPrograms[lastGoodEditorProgramIndex];
        }

        public void ResetLastGoodEditor()
        {
            lastGoodEditorProgramIndex = -1;
        }
    }
}