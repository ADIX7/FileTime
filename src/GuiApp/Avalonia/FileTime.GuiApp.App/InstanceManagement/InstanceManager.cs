using System.IO.Pipes;
using Avalonia.Controls;
using FileTime.GuiApp.App.InstanceManagement.Messages;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App.InstanceManagement;

public sealed class InstanceManager : IInstanceManager
{
    private const string PipeName = "FileTime.GuiApp";

    private readonly IInstanceMessageHandler _instanceMessageHandler;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _serverCancellationTokenSource = new();
    private Thread? _serverThread;
    private NamedPipeClientStream? _pipeClientStream;
    private readonly List<NamedPipeServerStream> _pipeServerStreams = new();

    [ActivatorUtilitiesConstructor]
    public InstanceManager(
        IInstanceMessageHandler instanceMessageHandler,
        ILogger<InstanceManager> logger)
    {
        _instanceMessageHandler = instanceMessageHandler;
        _logger = logger;
    }

    public InstanceManager(
        IInstanceMessageHandler instanceMessageHandler,
        ILogger logger)
    {
        _instanceMessageHandler = instanceMessageHandler;
        _logger = logger;
    }

    public async Task<bool> TryConnectAsync(CancellationToken token = default)
    {
        if (_pipeClientStream is not null) return true;

        _pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
        try
        {
            await _pipeClientStream.ConnectAsync(200, token);

        }
        catch
        {
            return false;
        }

        return true;
    }

    public Task ExitAsync(CancellationToken token = default)
    {
        _serverCancellationTokenSource.Cancel();

        var pipeServerStreams = _pipeServerStreams.ToArray();
        _pipeServerStreams.Clear();

        foreach (var pipeServerStream in pipeServerStreams)
        {
            try
            {
                pipeServerStream.Close();
                pipeServerStream.Dispose();
            }
            catch
            {
                // ignored
            }
        }


        return Task.CompletedTask;
    }

    public Task InitAsync()
    {
        _serverThread = new Thread(StartServer);
        _serverThread.Start();

        return Task.CompletedTask;
    }

    private async void StartServer()
    {
        try
        {
            if (await TryConnectAsync())
            {
                //An instance already exists, this one won't listen for connections
                return;
            }
            
            while (true)
            {
                var pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 200);
                _pipeServerStreams.Add(pipeServer);
                try
                {
                    await pipeServer.WaitForConnectionAsync(_serverCancellationTokenSource.Token);
                    ThreadPool.QueueUserWorkItem(HandleConnection, pipeServer);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in server thread");
        }
    }

    private async void HandleConnection(object? state)
    {
        if (state is not NamedPipeServerStream pipeServer)
            throw new ArgumentException(nameof(state) + "is not" + nameof(NamedPipeServerStream));

        while (true)
        {
            IInstanceMessage message;
            try
            {
                message = await ReadMessageAsync(pipeServer, _serverCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (MessagePackSerializationException)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading message");
                break;
            }

            try
            {
                await _instanceMessageHandler.HandleMessageAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while handling message");
                break;
            }
        }

        _pipeServerStreams.Remove(pipeServer);
        pipeServer.Close();
        await pipeServer.DisposeAsync();
    }

    public async Task SendMessageAsync<T>(T message, CancellationToken token = default) where T : class, IInstanceMessage
    {
        if (!await TryConnectAsync(token))
        {
            _logger.LogWarning("Could not connect to server, can send message {Message}", message);
            return;
        }

        await WriteMessageAsync(_pipeClientStream!, message, token);
        await _pipeClientStream!.FlushAsync(token);
    }

    private static async Task<IInstanceMessage> ReadMessageAsync(Stream stream, CancellationToken token = default)
        => await MessagePackSerializer.DeserializeAsync<IInstanceMessage>(stream, cancellationToken: token);

    private static async Task WriteMessageAsync(Stream stream, IInstanceMessage message, CancellationToken token = default)
        => await MessagePackSerializer.SerializeAsync(stream, message, cancellationToken: token);
}