using FileTime.Core.Components;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FileTime.Core.Timeline;
using FileTime.Core.Providers;
using Syroot.Windows.IO;
using FileTime.Avalonia.IconProviders;
using Microsoft.Extensions.Logging;
using System.Threading;
using Avalonia.Input;
using System.Reflection;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(AppState), PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(StatePersistenceService), PropertyName = "StatePersistence", PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(ItemNameConverterService))]
    [Inject(typeof(ILogger<MainPageViewModel>), PropertyName = "_logger")]
    [Inject(typeof(KeyboardConfigurationService))]
    [Inject(typeof(CommandHandlerService), PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(IDialogService), PropertyName = "_dialogService")]
    [Inject(typeof(KeyInputHandlerService))]
    public partial class MainPageViewModel : IMainPageViewModelBase
    {
        public const string RAPIDTRAVEL = "rapidTravel";

        private TimeRunner _timeRunner;

        [Property]
        private string _text;

        [Property]
        private List<RootDriveInfo> _rootDriveInfos;

        [Property]
        private List<PlaceInfo> _places;

        [Property]
        private bool _loading = true;

        public string Title { get; private set; }

        async partial void OnInitialize()
        {
            _logger?.LogInformation($"Starting {nameof(MainPageViewModel)} initialization...");

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = "Unknwon version";
            if (version != null)
            {
                versionString = $"{version.Major}.{version.Minor}.{version.Build}";
                if (version.Revision != 0)
                {
                    versionString += $" ({version.Revision})";
                }
            }
            Title = "FileTime " + versionString;

            _timeRunner = App.ServiceProvider.GetService<TimeRunner>()!;
            var inputInterface = (BasicInputHandler)App.ServiceProvider.GetService<IInputInterface>()!;
            inputInterface.InputHandler = _dialogService.ReadInputs;
            App.ServiceProvider.GetService<TopContainer>();
            await StatePersistence.LoadStatesAsync();

            _timeRunner.CommandsChangedAsync.Add(UpdateParallelCommands);

            if (AppState.Tabs.Count == 0)
            {
                var tab = new Tab();
                await tab.Init(LocalContentProvider);

                var tabContainer = new TabContainer(_timeRunner, tab, LocalContentProvider, ItemNameConverterService);
                await tabContainer.Init(1);
                tabContainer.IsSelected = true;
                AppState.Tabs.Add(tabContainer);
            }

            var driveInfos = new List<RootDriveInfo>();
            var drives = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed)
                : DriveInfo.GetDrives().Where(d =>
                    d.DriveType == DriveType.Fixed
                    && d.DriveFormat != "pstorefs"
                    && d.DriveFormat != "bpf_fs"
                    && d.DriveFormat != "tracefs"
                    && !d.RootDirectory.FullName.StartsWith("/snap/"));
            foreach (var drive in drives)
            {
                var container = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? await GetContainerForWindowsDrive(drive)
                    : await GetContainerForLinuxDrive(drive);
                if (container != null)
                {
                    var driveInfo = new RootDriveInfo(drive, container);
                    driveInfos.Add(driveInfo);
                }
            }
            RootDriveInfos = driveInfos.OrderBy(d => d.Name).ToList();

            var places = new List<PlaceInfo>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var placesFolders = new List<KnownFolder>()
                {
                    KnownFolders.Profile,
                    KnownFolders.Desktop,
                    KnownFolders.DocumentsLocalized,
                    KnownFolders.DownloadsLocalized,
                    KnownFolders.Music,
                    KnownFolders.Pictures,
                    KnownFolders.Videos,
                };

                foreach (var placesFolder in placesFolders)
                {
                    var possibleContainer = await LocalContentProvider.GetByPath(placesFolder.Path);
                    if (possibleContainer is IContainer container)
                    {
                        var name = container.Name;
                        if (await container.GetByPath("desktop.ini") is LocalFile element)
                        {
                            var lines = File.ReadAllLines(element.File.FullName);
                            if (Array.Find(lines, l => l.StartsWith("localizedresourcename", StringComparison.OrdinalIgnoreCase)) is string nameLine)
                            {
                                var nameLineValue = string.Join('=', nameLine.Split('=')[1..]);
                                var environemntVariables = Environment.GetEnvironmentVariables();
                                foreach (var keyo in environemntVariables.Keys)
                                {
                                    if (keyo is string key && environemntVariables[key] is string value)
                                    {
                                        nameLineValue = nameLineValue.Replace($"%{key}%", value);
                                    }
                                }

                                if (nameLineValue.StartsWith("@"))
                                {
                                    var parts = nameLineValue[1..].Split(',');
                                    if (parts.Length >= 2 && long.TryParse(parts[^1], out var parsedResourceId))
                                    {
                                        if (parsedResourceId < 0) parsedResourceId *= -1;

                                        name = NativeMethodHelpers.GetStringResource(string.Join(',', parts[..^1]), (uint)parsedResourceId);
                                    }
                                }
                                else
                                {
                                    name = nameLineValue;
                                }
                            }
                        }
                        places.Add(new PlaceInfo(name, container));
                    }

                }
                LocalContentProvider.Unload();
            }
            else
            {
                throw new Exception("TODO linux places");
            }
            Places = places;
            await Task.Delay(100);
            Loading = false;
            _logger?.LogInformation($"{nameof(MainPageViewModel)} initialized.");
        }

        private Task UpdateParallelCommands(object? sender, IReadOnlyList<ReadOnlyParallelCommands> parallelCommands, CancellationToken token)
        {
            foreach (var parallelCommand in parallelCommands)
            {
                if (!AppState.TimelineCommands.Any(c => c.Id == parallelCommand.Id))
                {
                    AppState.TimelineCommands.Add(new ParallelCommandsViewModel(parallelCommand));
                }
            }
            var itemsToRemove = new List<ParallelCommandsViewModel>();
            foreach (var parallelCommandVm in AppState.TimelineCommands)
            {
                if (!parallelCommands.Any(c => c.Id == parallelCommandVm.Id))
                {
                    itemsToRemove.Add(parallelCommandVm);
                }
            }

            for (var i = 0; i < itemsToRemove.Count; i++)
            {
                itemsToRemove[i].Destroy();
                AppState.TimelineCommands.Remove(itemsToRemove[i]);
            }

            foreach (var parallelCommand in parallelCommands)
            {
                var parallelCommandsVM = AppState.TimelineCommands.First(t => t.Id == parallelCommand.Id);
                foreach (var command in parallelCommand.Commands)
                {
                    if (!parallelCommandsVM.ParallelCommands.Any(c => c.CommandTimeState.Command == command.Command))
                    {
                        parallelCommandsVM.ParallelCommands.Add(new ParallelCommandViewModel(command));
                    }
                }

                var commandVMsToRemove = new List<ParallelCommandViewModel>();
                foreach (var commandVM in parallelCommandsVM.ParallelCommands)
                {
                    if (!parallelCommand.Commands.Any(c => c.Command == commandVM.CommandTimeState.Command))
                    {
                        commandVMsToRemove.Add(commandVM);
                    }
                }

                for (var i = 0; i < commandVMsToRemove.Count; i++)
                {
                    commandVMsToRemove[i].Destroy();
                    parallelCommandsVM.ParallelCommands.Remove(commandVMsToRemove[i]);
                }
            }

            return Task.CompletedTask;
        }

        private async Task<IContainer?> GetContainerForWindowsDrive(DriveInfo drive)
        {
            return (await LocalContentProvider.GetRootContainers()).FirstOrDefault(d => d.Name == drive.Name.TrimEnd(Path.DirectorySeparatorChar));
        }

        private async Task<IContainer?> GetContainerForLinuxDrive(DriveInfo drive)
        {
            return await LocalContentProvider.GetByPath(drive.Name) as IContainer;
        }

        [Command]
        public async void ProcessInputs()
        {
            await _dialogService.ProcessInputs();
        }

        [Command]
        public void CancelInputs()
        {
            _dialogService.CancelInputs();
        }

        [Command]
        public void ProcessMessageBox()
        {
            _dialogService.ProcessMessageBox();
        }

        [Command]
        public void CancelMessageBox()
        {
            _dialogService.CancelMessageBox();
        }

        public void ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
        {
            KeyInputHandlerService.ProcessKeyDown(key, keyModifiers, setHandled);
        }
    }
}
