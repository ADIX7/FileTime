using System.Reactive.Linq;
using System.Reactive.Subjects;
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

namespace FileTime.App.FrequencyNavigation.Services;

public partial class FrequencyNavigationService : IFrequencyNavigationService, IStartupHandler, IExitHandler
{
    private const int MaxAge = 10_000;

    private DateTime _lastSave = DateTime.Now;
    private readonly ILogger<FrequencyNavigationService> _logger;
    private readonly IModalService _modalService;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private Dictionary<string, ContainerFrequencyData> _containerScores = new();
    private readonly DeclarativeProperty<bool> _showWindow = new(false);
    private readonly string _dbPath;
    private bool _loaded;
    
    [Notify] IFrequencyNavigationViewModel? _currentModal;
    IDeclarativeProperty<bool> IFrequencyNavigationService.ShowWindow => _showWindow;

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
        await _saveLock.WaitAsync();
        try
        {
            if (container.GetExtension<NonRestorableContainerExtension>() is not null) return;

            var containerNameString = container.FullName?.Path;
            if (containerNameString is null) return;

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

            //TODO: move to if above
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
        try
        {
            return _containerScores
                .Where(c =>
                {
                    var searchTerms = searchText.Split(' ');
                    return searchTerms.All(s => c.Key.Contains(s, StringComparison.OrdinalIgnoreCase));
                })
                .OrderByDescending(c => GetWeightedScore(c.Value.Score, c.Value.LastAccessed))
                .Select(c => c.Key)
                .ToList();
        }
        finally
        {
            _saveLock.Release();
        }
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
        _loaded = true;
    }

    private async Task LoadStateAsync()
    {
        if (!File.Exists(_dbPath))
            return;

        try
        {
            await _saveLock.WaitAsync();
            _logger.LogTrace("Loading frequency navigation state from file '{DbPath}'", _dbPath);
            await using var dbStream = File.OpenRead(_dbPath);
            var containerScores = await JsonSerializer.DeserializeAsync<Dictionary<string, ContainerFrequencyData>>(dbStream);
            if (containerScores is null) return;

            _containerScores = containerScores;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading frequency navigation state");
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public async Task ExitAsync(CancellationToken token = default) => await SaveStateAsync(token);

    private async Task SaveStateAsync(CancellationToken token = default)
    {
        if(!_loaded) return;
        await _saveLock.WaitAsync(token);
        try
        {
            _lastSave = DateTime.Now;
            await using var dbStream = File.Create(_dbPath);
            await JsonSerializer.SerializeAsync(dbStream, _containerScores);
            dbStream.Flush();
        }
        finally
        {
            _saveLock.Release();
        }
    }
}