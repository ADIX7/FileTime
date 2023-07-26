using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FileTime.App.Core.Services;
using FileTime.Core.Interactions;
using FileTime.Providers.Local;
using FileTime.Server.Common;
using FileTime.Server.Common.Connections.SignalR;
using InitableService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileTime.Providers.LocalAdmin;

public class AdminElevationManager : IAdminElevationManager, INotifyPropertyChanged, IExitHandler
{
    private class ConnectionInfo
    {
        public string? SignalRBaseUrl { get; init; }
    }

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IUserCommunicationService _dialogService;
    private readonly ILogger<AdminElevationManager> _logger;
    private readonly IOptionsMonitor<AdminElevationConfiguration> _configuration;
    private readonly IServiceProvider _serviceProvider;
    private ConnectionInfo? _connectionInfo;
    private bool _isAdminInstanceRunning;
    private Process? _adminProcess;

    public bool IsAdminModeSupported => true;
    private bool StartProcess => _configuration.CurrentValue.StartProcess ?? true;

    public bool IsAdminInstanceRunning
    {
        get => _isAdminInstanceRunning;
        private set => SetField(ref _isAdminInstanceRunning, value);
    }

    public string ProviderName => LocalContentProviderConstants.ContentProviderId;

    public AdminElevationManager(
        IUserCommunicationService dialogService,
        ILogger<AdminElevationManager> logger,
        IOptionsMonitor<AdminElevationConfiguration> configuration,
        IServiceProvider serviceProvider
    )
    {
        _dialogService = dialogService;
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task CreateAdminInstanceIfNecessaryAsync(string? confirmationMessage = null)
    {
        ArgumentNullException.ThrowIfNull(_configuration.CurrentValue.ServerExecutablePath, "ServerExecutablePath");

        await _lock.WaitAsync();
        try
        {
            if (IsAdminInstanceRunning) return;

            confirmationMessage ??= "This operation requires admin privileges. Please confirm to continue.";
            var confirmationResult = await _dialogService.ShowMessageBox(confirmationMessage);
            if (confirmationResult == MessageBoxResult.Cancel) return;

            var port = _configuration.CurrentValue.ServerPort;
            _logger.LogTrace("Admin server port is {Port}", port is null ? "<not set>" : $"{port}");
            if (StartProcess || port is null)
            {
                var portFileName = Path.GetTempFileName();
                var process = new Process
                {
                    StartInfo = new()
                    {
                        FileName = _configuration.CurrentValue.ServerExecutablePath,
                        ArgumentList =
                        {
                            "--PortWriter:FileName",
                            portFileName
                        },
                        UseShellExecute = true,
                        Verb = "runas"
                    },
                    EnableRaisingEvents = true
                };
                process.Exited += ProcessExitHandler;
                process.Start();
                _adminProcess = process;

                IsAdminInstanceRunning = true;

                //TODO: timeout
                while (!File.Exists(portFileName) || new FileInfo(portFileName).Length == 0)
                    await Task.Delay(10);

                var content = await File.ReadAllLinesAsync(portFileName);
                if (int.TryParse(content.FirstOrDefault(), out var parsedPort))
                {
                    port = parsedPort;
                }
                else
                {
                    _logger.LogError(
                        "Could not parse port from content {Content}",
                        string.Join(Environment.NewLine, content)
                    );
                }
            }

            var connectionInfo = new ConnectionInfo
            {
                SignalRBaseUrl = $"http://localhost:{port}/RemoteHub"
            };

            _connectionInfo = connectionInfo;
        }
        catch (Exception ex)
        {
            IsAdminInstanceRunning = false;
            _logger.LogError(ex, "Error creating admin instance");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IRemoteConnection> CreateConnectionAsync()
    {
        ArgumentNullException.ThrowIfNull(_connectionInfo);
        try
        {
            //TODO: use other connections too (if there will be any)
            ArgumentNullException.ThrowIfNull(_connectionInfo.SignalRBaseUrl);

            var connection = await SignalRConnection.GetOrCreateForAsync(_connectionInfo.SignalRBaseUrl);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SignalR connection");
            throw;
        }
    }

    private void ProcessExitHandler(object? sender, EventArgs e)
    {
        _lock.Wait();
        IsAdminInstanceRunning = false;
        _lock.Release();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public async Task ExitAsync(CancellationToken token = default)
    {
        if (!StartProcess)
        {
            _logger.LogTrace("Not stopping admin process as it was not started by this instance");
            return;
        }

        if (!IsAdminInstanceRunning)
        {
            _logger.LogTrace("Not stopping admin process as it is not running");
            return;
        }

        try
        {
            _logger.LogInformation("Stopping admin process");
            var connection = await CreateConnectionAsync();
            await connection.Exit();
            _logger.LogInformation("Admin process stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping admin process");
            if (_adminProcess is null) return;

            for (var i = 0; i < 150 && !_adminProcess.HasExited; i++)
            {
                await Task.Delay(10);
            }

            /*if (!_adminProcess.HasExited)
            {
                _logger.LogInformation("Admin process dit not stopped, killing it");
                _adminProcess.Kill();
            }*/
        }
    }
}