using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local;

public class LocalContentWriterFactory : IContentWriterFactory<ILocalContentProvider>
{
    private readonly IAdminContentAccessorFactory _adminContentAccessorFactory;
    private readonly ILogger<LocalContentWriterFactory> _logger;

    public LocalContentWriterFactory(
        IAdminContentAccessorFactory adminContentAccessorFactory,
        ILogger<LocalContentWriterFactory> logger)
    {
        _adminContentAccessorFactory = adminContentAccessorFactory;
        _logger = logger;
    }
    
    public async Task<IContentWriter> CreateContentWriterAsync(IElement element)
    {
        try
        {
            return new LocalContentWriter(File.OpenWrite(element.NativePath!.Path));
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogDebug(e, "Failed to write to element with path {Path}", element.NativePath);
            var adminContentWriter = await _adminContentAccessorFactory.CreateContentWriterAsync(element.NativePath!);
            return adminContentWriter;
        }
    }
}