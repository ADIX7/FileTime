namespace FileTime.Core.Timeline
{
    public class ReadOnlyParallelCommands
    {
        public IReadOnlyList<ReadOnlyCommandTimeState> Commands { get; }
        public ReadOnlyParallelCommands(ParallelCommands parallelCommands)
        {
            Commands = parallelCommands.Commands.Select(c => new ReadOnlyCommandTimeState(c)).ToList().AsReadOnly();
        }
    }
}