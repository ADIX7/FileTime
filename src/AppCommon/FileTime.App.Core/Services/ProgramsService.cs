using System.Runtime.InteropServices;
using FileTime.App.Core.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.App.Core.Services;

public class ProgramsService : IProgramsService
{
    private enum Os
    {
        Linux,
        Windows,
    }

    private readonly Os _os;
    private readonly IOptionsMonitor<ProgramsConfigurationRoot> _configuration;
    private int _lastGoodEditorProgramIndex;
    private readonly List<ProgramConfiguration> _lastEditorPrograms = new();

    public ProgramsService(IOptionsMonitor<ProgramsConfigurationRoot> configuration)
    {
        _configuration = configuration;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _os = Os.Windows;
        }
        else
        {
            _os = Os.Linux;
        }
    }

    public ProgramConfiguration? GetEditorProgram(bool getNext = false)
    {
        GeneratePrograms();
        
        if (getNext)
        {
            _lastGoodEditorProgramIndex++;
        }

        if (_lastGoodEditorProgramIndex < 0)
        {
            _lastGoodEditorProgramIndex = 0;
        }

        if (_lastEditorPrograms.Count <= _lastGoodEditorProgramIndex)
        {
            ResetLastGoodEditor();
            return null;
        }

        return _lastEditorPrograms[_lastGoodEditorProgramIndex];
    }

    private void GeneratePrograms()
    {
        var programsConfig = _os switch
        {
            Os.Windows => _configuration.CurrentValue.Windows,
            _ => _configuration.CurrentValue.Linux
        };
        var programConfigs = programsConfig.EditorPrograms.Count == 0
            ? programsConfig.DefaultEditorPrograms
            : programsConfig.EditorPrograms.Concat(programsConfig.DefaultEditorPrograms).ToList();

        var generateNew = programConfigs.Count != _lastEditorPrograms.Count;

        if (!generateNew)
        {
            for (var i = 0; i < programConfigs.Count; i++)
            {
                if (programConfigs[i].Path != _lastEditorPrograms[i].Path
                    || programConfigs[i].Arguments != _lastEditorPrograms[i].Arguments)
                {
                    generateNew = true;
                    break;
                }
            }
        }

        if (generateNew)
        {
            _lastEditorPrograms.Clear();
            _lastEditorPrograms.AddRange(programConfigs);
            _lastGoodEditorProgramIndex = -1;
        }
    }

    public void ResetLastGoodEditor() => _lastGoodEditorProgramIndex = -1;
}