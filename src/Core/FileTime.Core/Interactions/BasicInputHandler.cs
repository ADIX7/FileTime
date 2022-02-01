namespace FileTime.Core.Interactions
{
    public class BasicInputHandler : IInputInterface
    {
        public Func<IEnumerable<InputElement>, Task<string?[]>>? InputHandler { get; set; }
        public async Task<string?[]> ReadInputs(IEnumerable<InputElement> fields) =>
            InputHandler != null ? await InputHandler.Invoke(fields) : throw new NotImplementedException(nameof(InputHandler) + " is not set");
    }
}