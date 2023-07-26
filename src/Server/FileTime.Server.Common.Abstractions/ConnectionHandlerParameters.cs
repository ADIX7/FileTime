using Autofac;
using Microsoft.Extensions.Configuration;

namespace FileTime.Server.Common;

public record ConnectionHandlerParameters(
    string[] Args,
    IContainer RootContainer,
    IConfigurationRoot ConfigurationRoot,
    CancellationToken ApplicationExit
);