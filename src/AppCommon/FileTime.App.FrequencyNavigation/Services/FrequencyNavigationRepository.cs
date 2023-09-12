using FileTime.App.Database;
using FileTime.App.FrequencyNavigation.Models;
using FileTime.Core.Models;

namespace FileTime.App.FrequencyNavigation.Services;

public class FrequencyNavigationRepository
{
    private class ContainerScore
    {
        public int Id { get; set; }
        public required string Path { get; init; }
        public required DateTime LastAccessed { get; set; }
        public required int Score { get; set; }
    }

    private const string CollectionName = "FrequencyNavigation";
    private const double CacheExpirationInSeconds = 60;
    private const double MaxPersistIntervalInSeconds = 60;

    private readonly IDatabaseContext _databaseContext;
    private readonly SemaphoreSlim _databaseSemaphore = new(1, 1);
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
    private readonly List<ContainerFrequencyData> _cachedFrequencyData = new();
    private readonly List<ContainerFrequencyData> _extraCachedFrequencyData = new();
    private DateTime _cachedTime;
    private DateTime _lastPersistTime = DateTime.Now;

    public FrequencyNavigationRepository(IDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task IncreaseContainerScoreAsync(string containerNameString)
    {
        if (!containerNameString.Contains(Constants.SeparatorChar)) return;

        await _cacheSemaphore.WaitAsync();
        try
        {
            var frequencyData = _extraCachedFrequencyData.FirstOrDefault(d => d.Path == containerNameString);
            if (frequencyData is null)
            {
                _extraCachedFrequencyData.Add(new ContainerFrequencyData(containerNameString, 1, DateTime.Now));
            }
            else
            {
                frequencyData.Score++;
                frequencyData.LastAccessed = DateTime.Now;
            }
        }
        finally
        {
            _cacheSemaphore.Release();
        }

        await TryPersistExtraFrequencyDataAsync();
    }

    private async Task TryPersistExtraFrequencyDataAsync()
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            if ((DateTime.Now - _lastPersistTime).TotalSeconds < MaxPersistIntervalInSeconds)
            {
                return;
            }

            _lastPersistTime = DateTime.Now;
        }
        finally
        {
            _cacheSemaphore.Release();
        }

        await PersistExtraFrequencyDataAsync();
    }

    private async Task PersistExtraFrequencyDataAsync(bool skipCacheLock = false)
    {
        await _databaseSemaphore.WaitAsync();
        if (!skipCacheLock)
        {
            await _cacheSemaphore.WaitAsync();
        }

        try
        {
            using var connection = await _databaseContext.GetConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var queryCollection = connection.GetCollection<ContainerScore>(CollectionName);
            var updateCollection = transaction.GetCollection<ContainerScore>(CollectionName);

            var extraCachedFrequencyData = _extraCachedFrequencyData.ToList();
            _extraCachedFrequencyData.Clear();
            foreach (var extraFrequencyData in extraCachedFrequencyData)
            {
                var currentFrequencyData = queryCollection.FirstOrDefault(d => d.Path == extraFrequencyData.Path);

                if (currentFrequencyData is null)
                {
                    updateCollection.Insert(new ContainerScore
                    {
                        Path = extraFrequencyData.Path,
                        LastAccessed = extraFrequencyData.LastAccessed,
                        Score = extraFrequencyData.Score
                    });
                }
                else
                {
                    currentFrequencyData.Score += extraFrequencyData.Score;
                    currentFrequencyData.LastAccessed = extraFrequencyData.LastAccessed;

                    updateCollection.Update(currentFrequencyData);
                }
            }

            await transaction.CommitAsync();
        }
        finally
        {
            _databaseSemaphore.Release();
            if (!skipCacheLock)
            {
                _cacheSemaphore.Release();
            }
        }
    }

    public async Task<int> GetAgeSum()
    {
        await _databaseSemaphore.WaitAsync();
        try
        {
            using var connection = await _databaseContext.GetConnectionAsync();
            var query = connection.GetCollection<ContainerScore>(CollectionName);
            return query.Query().Select(c => c.Score).ToEnumerable().Sum();
        }
        finally
        {
            _databaseSemaphore.Release();
        }
    }

    public async Task AgeContainersAsync()
    {
        await PersistExtraFrequencyDataAsync();

        await _databaseSemaphore.WaitAsync();
        try
        {
            using var connection = await _databaseContext.GetConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var queryCollection = connection.GetCollection<ContainerScore>(CollectionName);
            var updateCollection = transaction.GetCollection<ContainerScore>(CollectionName);

            var now = DateTime.Now;
            foreach (var container in queryCollection.ToEnumerable())
            {
                var newScore = (int) Math.Floor(container.Score * 0.9);
                if (newScore > 0)
                {
                    container.Score = newScore;
                    container.LastAccessed = now;
                    updateCollection.Update(container);
                }
                else
                {
                    updateCollection.Delete(container.Id);
                }
            }

            await transaction.CommitAsync();
        }
        finally
        {
            _databaseSemaphore.Release();
        }
    }

    public async ValueTask<ICollection<ContainerFrequencyData>> GetContainersAsync()
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            if ((DateTime.Now - _cachedTime).TotalSeconds > CacheExpirationInSeconds)
            {
                await PersistExtraFrequencyDataAsync(skipCacheLock: true);
                var containerScores = await GetContainersFromDatabaseAsync();
                _cachedFrequencyData.Clear();
                _cachedFrequencyData.AddRange(containerScores);
                _cachedTime = DateTime.Now;

                return _cachedFrequencyData.ToArray();
            }

            var frequencyData = new List<ContainerFrequencyData>(_cachedFrequencyData);

            foreach (var extraFrequencyData in _extraCachedFrequencyData)
            {
                var existingFrequencyData = frequencyData
                    .FirstOrDefault(f => f.Path == extraFrequencyData.Path);
                if (existingFrequencyData is null)
                {
                    frequencyData.Add(extraFrequencyData);
                }
                else
                {
                    existingFrequencyData.Score += extraFrequencyData.Score;
                    existingFrequencyData.LastAccessed = extraFrequencyData.LastAccessed;
                }
            }


            return frequencyData;
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    private async Task<IEnumerable<ContainerFrequencyData>> GetContainersFromDatabaseAsync()
    {
        await _databaseSemaphore.WaitAsync();
        try
        {
            using var connection = await _databaseContext.GetConnectionAsync();

            var repo = connection.GetCollection<ContainerScore>(CollectionName);

            return repo
                .ToEnumerable()
                .Select(c => new ContainerFrequencyData(c.Path, c.Score, c.LastAccessed))
                .ToList();
        }
        finally
        {
            _databaseSemaphore.Release();
        }
    }

    public async Task AddContainerAsync(ContainerFrequencyData containerFrequencyData)
    {
        await _databaseSemaphore.WaitAsync();
        try
        {
            using var connection = await _databaseContext.GetConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var repo = transaction.GetCollection<ContainerScore>(CollectionName);

            repo.Insert(new ContainerScore
            {
                Path = containerFrequencyData.Path,
                LastAccessed = containerFrequencyData.LastAccessed,
                Score = containerFrequencyData.Score
            });

            await transaction.CommitAsync();
        }
        finally
        {
            _databaseSemaphore.Release();
        }
    }

    public async Task AddContainersAsync(IEnumerable<ContainerFrequencyData> containerFrequencyData)
    {
        await _databaseSemaphore.WaitAsync();
        try
        {
            var containerFrequencyDataList = containerFrequencyData.ToList();
            if (containerFrequencyDataList.Count == 0) return;

            using var connection = await _databaseContext.GetConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var repo = transaction.GetCollection<ContainerScore>(CollectionName);

            foreach (var frequencyData in containerFrequencyDataList)
            {
                repo.Insert(new ContainerScore
                {
                    Path = frequencyData.Path,
                    LastAccessed = frequencyData.LastAccessed,
                    Score = frequencyData.Score
                });
            }

            await transaction.CommitAsync();
        }
        finally
        {
            _databaseSemaphore.Release();
        }
    }
}