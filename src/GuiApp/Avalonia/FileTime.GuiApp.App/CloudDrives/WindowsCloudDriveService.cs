// Based on: https://github.com/files-community/Files/blob/main/src/Files.App/Utils/Cloud/CloudDrivesDetector.cs

using System.Runtime.Versioning;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using Microsoft.Win32;
using PropertyChanged.SourceGenerator;

namespace FileTime.GuiApp.App.CloudDrives;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsCloudDriveService : ICloudDriveService
{
    [Notify] private IReadOnlyList<CloudDrive> _cloudDrives = new List<CloudDrive>();

    private async Task<List<CloudDrive>> GetCloudDrives()
    {
        var cloudDrives = new List<CloudDrive>();
        cloudDrives.AddRange(await GetOneDrive());
        cloudDrives.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        return cloudDrives;
    }

    private static Task<IEnumerable<CloudDrive>> GetOneDrive()
    {
        using var oneDriveAccountsKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\OneDrive\Accounts");
        if (oneDriveAccountsKey is null)
        {
            return Task.FromResult(Enumerable.Empty<CloudDrive>());
        }

        var oneDriveAccounts = new List<CloudDrive>();
        foreach (var account in oneDriveAccountsKey.GetSubKeyNames())
        {
            var accountKeyName = @$"{oneDriveAccountsKey.Name}\{account}";
            var displayName = (string?) Registry.GetValue(accountKeyName, "DisplayName", null);
            var userFolder = (string?) Registry.GetValue(accountKeyName, "UserFolder", null);
            var accountName = string.IsNullOrWhiteSpace(displayName) ? "OneDrive" : $"OneDrive - {displayName}";

            if (string.IsNullOrWhiteSpace(userFolder) || oneDriveAccounts.Any(x => x.Name == accountName)) continue;

            userFolder = PathHelper.ReplaceEnvironmentVariablePlaceHolders(userFolder);

            oneDriveAccounts.Add(new CloudDrive(accountName, new NativePath(userFolder)));
        }

        return Task.FromResult<IEnumerable<CloudDrive>>(oneDriveAccounts);
    }

    public async Task InitAsync() => CloudDrives = (await GetCloudDrives()).AsReadOnly();
}