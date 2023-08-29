using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileTime.Server.Web;

public class PortWriterService : IHostedService
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IOptions<PortWriterConfiguration> _configuration;
    private readonly ILogger<PortWriterService> _logger;

    public PortWriterService(
        IServer server,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<PortWriterConfiguration> configuration,
        ILogger<PortWriterService> logger)
    {
        _server = server;
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
        _logger = logger;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(WritePort);

        return Task.CompletedTask;
    }

    private void WritePort()
    {
        try
        {
            var filename = _configuration.Value.Filename;
            if (filename is null)
            {
                _logger.LogWarning("Could not save port to file as there were no file name given");
                return;
            }

            var address = GetAddress();
            if (address is null)
            {
                _logger.LogError("Could not get address");
                return;
            }

            var couldParsePort = int.TryParse(address.Split(':').LastOrDefault(), out var port);
            if (!couldParsePort)
            {
                _logger.LogError("Could not parse port from address {Address}", address);
                return;
            }

            _logger.LogInformation("Writing port to {PortFile}", filename);
            using var tempFileStream = File.CreateText(filename);
            tempFileStream.Write(port.ToString());

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                try
                {
                    _logger.LogInformation("Deleting port file {PortFile}", filename);
                    File.Delete(filename);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while deleting port file {PortFile}", filename);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save port to file");
        }
    }

    private string? GetAddress()
    {
        var features = _server.Features;
        var addresses = features.Get<IServerAddressesFeature>();
        var address = addresses?.Addresses.FirstOrDefault();
        return address;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}