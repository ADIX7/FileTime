namespace FileTime.Core.Timeline
{
    public class ReadOnlyParallelCommands
    {
        public IReadOnlyList<ReadOnlyCommandTimeState> Commands { get; }
        public ushort Id { get; }

        public ReadOnlyParallelCommands(ParallelCommands parallelCommands)
        {
            Commands = parallelCommands.Commands.Select(c => new ReadOnlyCommandTimeState(c)).ToList().AsReadOnly();
            Id = parallelCommands.Id;
        }
    }
}