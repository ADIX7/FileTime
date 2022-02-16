using AsyncEvent;
using FileTime.Core.Command;
using FileTime.Core.Models;
using Microsoft.Extensions.Logging;

namespace FileTime.Core.Timeline
{
    public class TimeRunner
    {
        private readonly CommandExecutor _commandExecutor;
        private readonly List<ParallelCommands> _commandsToRun = new();
        private readonly object _guard = new();
        private readonly ILogger<TimeRunner> _logger;

        private bool _resourceIsInUse;
        private readonly List<CommandRunner> _commandRunners = new();
        private bool _enableRunning = true;

        private IReadOnlyList<ReadOnlyParallelCommands> _parallelCommands = new List<ReadOnlyParallelCommands>();

        public bool EnableRunning
        {
            get
            {
                bool result = true;
                RunWithLock(() => result = _enableRunning);
                return result;
            }

            set
            {
                RunWithLock(() => _enableRunning = value);
            }
        }

        public async Task<IReadOnlyList<ReadOnlyParallelCommands>> GetParallelCommandsAsync()
        {
            IReadOnlyList<ReadOnlyParallelCommands> parallelCommands = new List<ReadOnlyParallelCommands>();
            await RunWithLockAsync(() => parallelCommands = _parallelCommands);

            return parallelCommands;
        }

        public AsyncEventHandler<AbsolutePath> RefreshContainer { get; } = new AsyncEventHandler<AbsolutePath>();

        public AsyncEventHandler<IReadOnlyList<ReadOnlyParallelCommands>> CommandsChangedAsync { get; } = new();

        public TimeRunner(CommandExecutor commandExecutor, ILogger<TimeRunner> logger)
        {
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public async Task AddCommand(ICommand command, int? batchId = null, bool toNewBatch = false)
        {
            await RunWithLockAsync(async () =>
            {
                ParallelCommands batchToAdd;

                if (_commandsToRun.Count == 0)
                {
                    batchToAdd = new ParallelCommands(PointInTime.CreateEmpty());
                    _commandsToRun.Add(batchToAdd);
                }
                else if (toNewBatch)
                {
                    batchToAdd = new ParallelCommands(_commandsToRun.Last().Result);
                    _commandsToRun.Add(batchToAdd);
                }
                else if (batchId != null && _commandsToRun.Find(b => b.Id == batchId) is ParallelCommands parallelCommands)
                {
                    batchToAdd = parallelCommands;
                }
                else
                {
                    batchToAdd = _commandsToRun[0];
                }
                await batchToAdd.AddCommand(command);

                await RefreshCommands();

                if (_commandRunners.Count == 0)
                {
                    StartCommandRunner();
                }

                await UpdateReadOnlyCommands();
            });
        }

        public async Task TryStartCommandRunner()
        {
            await RunWithLockAsync(() => StartCommandRunner());
        }

        private void StartCommandRunner()
        {
            if (_enableRunning)
            {
                RunCommands();
            }
        }

        private void RunCommands()
        {
            while (_commandsToRun.Count > 0 && _commandsToRun[0].Commands.Count == 0) _commandsToRun.RemoveAt(0);

            if (_commandsToRun.Count > 0)
            {
                foreach (var command in _commandsToRun[0].Commands)
                {
                    if (_commandRunners.Find(r => r.Command == command) != null)
                    {
                        continue;
                    }
                    else if (command.CanRun == CanCommandRun.True || (command.CanRun == CanCommandRun.Forceable && command.ForceRun))
                    {
                        _logger.LogDebug("Starting command: {0}", command.Command.DisplayLabel);
                        var thread = new Thread(new ParameterizedThreadStart(RunCommand));
                        _commandRunners.Add(new CommandRunner(thread, command));
                        thread.Start(command);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void RunCommand(object? arg)
        {
            CommandTimeState? commandToRun = null;
            try
            {
                if (arg is CommandTimeState commandToRun2)
                {
                    commandToRun = commandToRun2;
                    _commandExecutor.ExecuteCommandAsync(commandToRun.Command, this).Wait();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Error while running command: {CommandType} ({Command}) {Error}.",
                    commandToRun?.Command.GetType().Name,
                    commandToRun?.Command.DisplayLabel,
                    e.Message);
            }
            finally
            {
                DisposeCommandThread(Thread.CurrentThread, commandToRun).Wait();
            }
        }

        private async Task DisposeCommandThread(Thread thread, CommandTimeState? command)
        {
            await RunWithLockAsync(async () =>
            {
                if (command != null)
                {
                    _logger.LogDebug("Command finished running: {Command}", command.Command.DisplayLabel);
                    _commandsToRun[0].Remove(command);
                    if (_commandsToRun[0].Commands.Count == 0)
                    {
                        _logger.LogDebug("Removing empty command array. {RemainingBatchNumber} batch left.", _commandsToRun.Count - 1);
                        _commandsToRun.RemoveAt(0);
                    }
                }

                var currentCommandRunner = _commandRunners.Find(r => r.Thread == thread);
                if (currentCommandRunner != null) _commandRunners.Remove(currentCommandRunner);
                await UpdateReadOnlyCommands();
                StartCommandRunner();
            });
        }

        public async Task Refresh()
        {
            await RunWithLockAsync(async () =>
            {
                await RefreshCommands(PointInTime.CreateEmpty());
                await UpdateReadOnlyCommands();
            });
        }

        private async Task RefreshCommands(PointInTime? fullStartTime = null)
        {
            var currentTime = fullStartTime ?? _commandsToRun[0].Result;
            var startIndex = fullStartTime == null ? 1 : 0;

            for (var i = startIndex; i < _commandsToRun.Count; i++)
            {
                currentTime = await _commandsToRun[i].RefreshResult(currentTime);
            }
        }

        private async Task UpdateReadOnlyCommands()
        {
            var wait = _commandsToRun.Count == 1;
            if (wait) await Task.Delay(100);
            _parallelCommands = _commandsToRun.ConvertAll(c => new ReadOnlyParallelCommands(c)).AsReadOnly();
            await CommandsChangedAsync.InvokeAsync(this, _parallelCommands);
        }

        private async Task RunWithLockAsync(Action action)
        {
            await RunWithLockAsync(() => { action(); return Task.CompletedTask; });
        }

        private async Task RunWithLockAsync(Func<Task> func)
        {
            while (true)
            {
                lock (_guard)
                {
                    if (!_resourceIsInUse)
                    {
                        _resourceIsInUse = true;
                        break;
                    }
                }

                await Task.Delay(1);
            }

            try
            {
                await func();
            }
            finally
            {
                lock (_guard)
                {
                    _resourceIsInUse = false;
                }
            }
        }

        private void RunWithLock(Action action) => RunWithLockAsync(action).Wait();
    }
}