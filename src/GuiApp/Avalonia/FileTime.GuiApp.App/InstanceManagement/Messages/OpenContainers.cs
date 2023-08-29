using MessagePack;

namespace FileTime.GuiApp.App.InstanceManagement.Messages;

[MessagePackObject]
public class OpenContainers : IInstanceMessage
{
    public OpenContainers()
    {
        
    }

    public OpenContainers(IEnumerable<string> containers)
    {
        Containers.AddRange(containers);
    }
    
    [Key(0)]
    public List<string> Containers { get; set; } = new();
}