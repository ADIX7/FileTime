using System.Text.Json;
using DeclarativeProperty;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.FrequencyNavigation.Models;
using FileTime.App.FrequencyNavigation.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.Core.Services;
using Microsoft.Extensions.Logging;
using PropertyChanged.SourceGenerator;
using static System.DeferTools;

namespace FileTime.App.FrequencyNavigation.Services;

public partial class FrequencyNavigationService : IFrequencyNavigationService, IStartupHandler
{
    private const int MaxAge = 10_000;
    private const int AgingPersistenceAfterSeconds = 60;

    private readonly ILogger<FrequencyNavigationService> _logger;
    private readonly IModalService _modalService;
    private readonly FrequencyNavigationRepository _frequencyNavigationRepository;
    private readonly DeclarativeProperty<bool> _showWindow = new(false);
    private readonly string _oldDbPath;
    private DateTime _lastTryAging = DateTime.Now;

    [Notify] private IFrequencyNavigationViewModel? _currentModal;
    IDeclarativeProperty<bool> IFrequencyNavigationService.ShowWindow => _showWindow;

    public FrequencyNavigationService(
        ITabEvents tabEvents,
        IModalService modalService,
        FrequencyNavigationRepository frequencyNavigationRepository,
        IApplicationSettings applicationSettings,
        ILogger<FrequencyNavigationService> logger)
    {
        _logger = logger;
        _modalService = modalService;
        _frequencyNavigationRepository = frequencyNavigationRepository;
        tabEvents.LocationChanged += OnTabLocationChanged;

        _oldDbPath = Path.Combine(applicationSettings.AppDataRoot, "frequencyNavigationScores.json");
    }

    async void OnTabLocationChanged(object? sender, TabLocationChanged e)
    {
        try
        {
            await IncreaseContainerScore(e.Location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error while increasing container score");
        }
    }

    public async Task OpenNavigationWindow()
    {
        await _showWindow.SetValue(true);
        CurrentModal = _modalService.OpenModal<IFrequencyNavigationViewModel>();
    }

    public void CloseNavigationWindow()
    {
        _showWindow.SetValueSafe(false);
        if (_currentModal is not null)
        {
            _modalService.CloseModal(_currentModal);
            CurrentModal = null;
        }
    }

    private async Task IncreaseContainerScore(IContainer container)
    {
        try
        {
            if (container.GetExtension<NonRestorableContainerExtension>() is not null) return;

            var containerNameString = container.FullName?.Path;
            if (containerNameString is null) return;

            await _frequencyNavigationRepository.IncreaseContainerScoreAsync(containerNameString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error increasing container score");
        }

        await TryAgeContainersAsync();
    }

    private async Task TryAgeContainersAsync()
    {
        if ((DateTime.Now - _lastTryAging).TotalSeconds >= AgingPersistenceAfterSeconds)
        {
            return;
        }

        if (await _frequencyNavigationRepository.GetAgeSum() < MaxAge)
        {
            _lastTryAging = DateTime.Now;
            return;
        }

        await _frequencyNavigationRepository.AgeContainersAsync();
    }

    public async ValueTask<IList<string>> GetMatchingContainers(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<string>();

        var frequencyData = await _frequencyNavigationRepository.GetContainersAsync();
        var searchTerms = searchText.Split(' ');

        return frequencyData
            .Where(c => searchTerms.All(s => c.Path.Contains(s, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(c => GetWeightedScore(c.Score, c.LastAccessed))
            .Select(c => c.Path)
            .ToList();
    }

    private static int GetWeightedScore(int score, DateTime lastAccess)
    {
        var now = DateTime.Now;
        var timeSinceLastAccess = now - lastAccess;
        return timeSinceLastAccess.TotalHours switch
        {
            < 1 => score * 4,
            < 24 => score * 2,
            < 168 => score / 2,
            _ => score / 4
        };
    }

    // TODO: remove this migration at some time in the future
    public async Task InitAsync()
    {
        if (!File.Exists(_oldDbPath)) return;
        using var _ = Defer(() => File.Delete(_oldDbPath));

        await using var dbStream = File.OpenRead(_oldDbPath);
        var containerScores = await JsonSerializer.DeserializeAsync<Dictionary<string, OldContainerFrequencyData>>(dbStream);
        if (containerScores is null || containerScores.Count == 0)
        {
            return;
        }

        var frequencyData = containerScores
            .Select(
                c => new ContainerFrequencyData(c.Key, c.Value.Score, c.Value.LastAccessed)
            )
            .Where(c => c.Path.Contains(Constants.SeparatorChar));

        await _frequencyNavigationRepository.AddContainersAsync(frequencyData);
    }

    private class OldContainerFrequencyData
    {
        public int Score { get; set; } = 1;
        public DateTime LastAccessed { get; set; } = DateTime.Now;
    }
}