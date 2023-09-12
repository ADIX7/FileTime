namespace FileTime.App.FrequencyNavigation.Models;

public class ContainerFrequencyData
{
    public string Path { get; }
    public int Score { get; set; }
    public DateTime LastAccessed { get; set; }
    
    public ContainerFrequencyData(string path, int score, DateTime lastAccessed)
    {
        Path = path;
        Score = score;
        LastAccessed = lastAccessed;
    }
}