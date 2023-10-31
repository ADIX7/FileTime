using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileTime.Server.Web;

public class PortWriterService(IServer server,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<PortWriterConfiguration> configuration,
        ILogger<PortWriterService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        hostApplicationLifetime.ApplicationStarted.Register(WritePort);

        return Task.CompletedTask;
    }

    private void WritePort()
    {
        try
        {
            var filename = configuration.Value.Filename;
            if (filename is null)
            {
                logger.LogWarning("Could not save port to file as there were no file name given");
                return;
            }

            var address = GetAddress();
            if (address is null)
            {
                logger.LogError("Could not get address");
                return;
            }

            var couldParsePort = int.TryParse(address.Split(':').LastOrDefault(), out var port);
            if (!couldParsePort)
            {
                logger.LogError("Could not parse port from address {Address}", address);
                return;
            }

            logger.LogInformation("Writing port to {PortFile}", filename);
            using var tempFileStream = File.CreateText(filename);
            tempFileStream.Write(port.ToString());

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                try
                {
                    logger.LogInformation("Deleting port file {PortFile}", filename);
                    File.Delete(filename);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while deleting port file {PortFile}", filename);
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not save port to file");
        }
    }

    private string? GetAddress()
    {
        var features = server.Features;
        var addresses = features.Get<IServerAddressesFeature>();
        var address = addresses?.Addresses.FirstOrDefault();
        return address;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}