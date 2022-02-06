using AsyncEvent;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline
{
    public class TimeRunner
    {
        private readonly CommandExecutor _commandExecutor;
        private readonly List<ParallelCommands> _commandsToRun = new();
        private readonly object _guard = new();

        private bool _resourceIsInUse;
        private readonly List<Thread> _commandRunners = new();
        private bool _enableRunning; //= true;

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

        public AsyncEventHandler CommandsChangedAsync { get; } = new();

        public TimeRunner(CommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        public async Task AddCommand(ICommand command, ParallelCommands? batch = null, bool toNewBatch = false)
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
                else if (batch != null && _commandsToRun.Contains(batch))
                {
                    batchToAdd = batch;
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
            });

            await UpdateReadOnlyCommands();
        }

        public async Task TryStartCommandRunner()
        {
            await RunWithLockAsync(() =>
            {
                if (_commandRunners.Count == 0 && _commandsToRun.Count > 0)
                {
                    StartCommandRunner();
                }
            });
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
                    if (command.CanRun == CanCommandRun.True || (command.CanRun == CanCommandRun.Forceable && command.ForceRun))
                    {
                        var thread = new Thread(new ParameterizedThreadStart(RunCommand));
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
            finally
            {
                DisposeCommandThread(Thread.CurrentThread, commandToRun).Wait();
            }
        }

        private async Task DisposeCommandThread(Thread thread, CommandTimeState? command)
        {
            await RunWithLockAsync(() =>
            {
                if (command != null)
                {
                    _commandsToRun[0].Remove(command);
                    if (_commandsToRun[0].Commands.Count == 0)
                    {
                        _commandsToRun.RemoveAt(0);
                    }
                }

                _commandRunners.Remove(thread);
            });
            await UpdateReadOnlyCommands();

            await TryStartCommandRunner();
        }

        public async Task Refresh()
        {
            await RunWithLockAsync(async () => await RefreshCommands(PointInTime.CreateEmpty()));
            await UpdateReadOnlyCommands();
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
            var wait = false;
            await RunWithLockAsync(() => wait = _commandsToRun.Count == 1);
            if (wait) await Task.Delay(100);
            await RunWithLockAsync(() => _parallelCommands = _commandsToRun.ConvertAll(c => new ReadOnlyParallelCommands(c)).AsReadOnly());
            await CommandsChangedAsync.InvokeAsync(this, AsyncEventArgs.Empty);
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