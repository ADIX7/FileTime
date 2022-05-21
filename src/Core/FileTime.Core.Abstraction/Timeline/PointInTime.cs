namespace FileTime.Core.Timeline;

public class PointInTime
{
    private readonly List<Difference> _differences;
    public static readonly PointInTime Eternal = new PointInTime();
    public static readonly PointInTime Present = new PointInTime();

    public IReadOnlyList<Difference> Differences { get; }

    private PointInTime() : this(new List<Difference>())
    {
    }

    private PointInTime(IEnumerable<Difference> differences)
    {
        _differences = new List<Difference>(differences);
        Differences = _differences.AsReadOnly();
    }

    private PointInTime(PointInTime previous, IEnumerable<Difference> differences)
        : this(MergeDifferences(previous.Differences, differences))
    {
    }

    public PointInTime WithDifferences(IEnumerable<Difference> differences) =>
        new(this, differences);

    private static List<Difference> MergeDifferences(IEnumerable<Difference> previouses,
        IEnumerable<Difference> differences)
    {
        var merged = new List<Difference>();

        merged.AddRange(previouses);
        merged.AddRange(differences);

        return merged;
    }

    public static PointInTime CreateEmpty() => new PointInTime();
}