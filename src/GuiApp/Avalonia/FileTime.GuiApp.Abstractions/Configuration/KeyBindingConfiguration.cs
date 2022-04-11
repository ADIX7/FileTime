namespace FileTime.GuiApp.Configuration
{
    public class KeyBindingConfiguration
    {
        public bool UseDefaultBindings { get; set; } = true;
        public List<CommandBindingConfiguration> DefaultKeyBindings { get; set; } = new();
        public List<CommandBindingConfiguration> KeyBindings { get; set; } = new();
    }
}