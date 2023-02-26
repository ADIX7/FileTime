using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.FrequencyNavigation.Models;
using FileTime.App.FrequencyNavigation.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using Microsoft.Extensions.Logging;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.FrequencyNavigation.Services;

public partial class FrequencyNavigationService : IFrequencyNavigationService, IStartupHandler, IExitHandler
{
    private const int MaxAge = 10_000;

    private DateTime _lastSave = DateTime.Now;
    private readonly ILogger<FrequencyNavigationService> _logger;
    private readonly IModalService _modalService;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private Dictionary<string, ContainerFrequencyData> _containerScores = new();
    private readonly BehaviorSubject<bool> _showWindow = new(false);
    private readonly string _dbPath;
    [Notify] IFrequencyNavigationViewModel? _currentModal;
    IObservable<bool> IFrequencyNavigationService.ShowWindow => _showWindow.AsObservable();

    public FrequencyNavigationService(
        ITabEvents tabEvents,
        IApplicationSettings applicationSettings,
        ILogger<FrequencyNavigationService> logger,
        IModalService modalService)
    {
        _logger = logger;
        _modalService = modalService;
        _dbPath = Path.Combine(applicationSettings.AppDataRoot, "frequencyNavigationScores.json");
        tabEvents.LocationChanged += OnTabLocationChanged;
    }

    void OnTabLocationChanged(object? sender, TabLocationChanged e)
    {
        IncreaseContainerScore(e.Location);
    }

    public void OpenNavigationWindow()
    {
        _showWindow.OnNext(true);
        CurrentModal = _modalService.OpenModal<IFrequencyNavigationViewModel>();
    }

    public void CloseNavigationWindow()
    {
        _showWindow.OnNext(false);
        if (_currentModal is not null)
        {
            _modalService.CloseModal(_currentModal);
            CurrentModal = null;
        }
    }

    private async void IncreaseContainerScore(FullName containerName)
    {
        await _saveLock.WaitAsync();
        try
        {
            var containerNameString = containerName.Path;
            if (_containerScores.ContainsKey(containerNameString))
            {
                _containerScores[containerNameString].Score++;
                _containerScores[containerNameString].LastAccessed = DateTime.Now;
            }
            else
            {
                _containerScores.Add(containerNameString, new ContainerFrequencyData());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error increasing container score");
        }
        finally
        {
            _saveLock.Release();
        }

        try
        {
            if (TryAgeContainerScores() || DateTime.Now - _lastSave > TimeSpan.FromMinutes(5))
            {
            }

            await SaveStateAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error aging container scores");
        }
    }

    private bool TryAgeContainerScores()
    {
        if (_containerScores.Select(c => c.Value.Score).Sum() < MaxAge)
            return false;

        AgeContainerScores();
        return true;
    }

    private void AgeContainerScores()
    {
        var now = DateTime.Now;
        var itemsToRemove = new List<string>();
        foreach (var container in _containerScores)
        {
            var newScore = (int) Math.Floor(container.Value.Score * 0.9);
            if (newScore > 0)
            {
                container.Value.Score = newScore;
            }
            else
            {
                itemsToRemove.Add(container.Key);
            }
        }

        foreach (var itemToRemove in itemsToRemove)
        {
            _containerScores.Remove(itemToRemove);
        }
    }

    public IList<string> GetMatchingContainers(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<string>();

        _saveLock.Wait();
        var matchingContainers = _containerScores
            .Where(c => c.Key.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => GetWeightedScore(c.Value.Score, c.Value.LastAccessed))
            .Select(c => c.Key)
            .ToList();

        _saveLock.Release();
        return matchingContainers;
    }

    private int GetWeightedScore(int score, DateTime lastAccess)
    {
        var now = DateTime.Now;
        var timeSinceLastAccess = now - lastAccess;
        return timeSinceLastAccess.TotalHours switch
        {
            < 1 => score *= 4,
            < 24 => score *= 2,
            < 168 => score /= 2,
            _ => score /= 4
        };
    }

    public async Task InitAsync()
    {
        await LoadStateAsync();
    }

    private async Task LoadStateAsync()
    {
        if (!File.Exists(_dbPath))
            return;

        await _saveLock.WaitAsync();
        await using var dbStream = File.OpenRead(_dbPath);
        var containerScores = await JsonSerializer.DeserializeAsync<Dictionary<string, ContainerFrequencyData>>(dbStream);
        if (containerScores is null) return;

        _containerScores = containerScores;
        _saveLock.Release();
    }

    public async Task ExitAsync()
    {
        await SaveStateAsync();
    }

    private async Task SaveStateAsync()
    {
        await _saveLock.WaitAsync();
        _lastSave = DateTime.Now;
        await using var dbStream = File.OpenWrite(_dbPath);
        await JsonSerializer.SerializeAsync(dbStream, _containerScores);
        _saveLock.Release();
    }
}