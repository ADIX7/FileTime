using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public sealed class PointInTime
    {
        private readonly List<Difference> _differences;

        public IReadOnlyList<Difference> Differences { get; }

        public IContentProvider? Provider { get; }

        private PointInTime() : this(new List<Difference>(), null) { }

        private PointInTime(IEnumerable<Difference> differences, IContentProvider? provider)
        {
            _differences = new List<Difference>(differences);
            Differences = _differences.AsReadOnly();
            Provider = provider;
        }

        private PointInTime(PointInTime previous, IEnumerable<Difference> differences, IContentProvider provider)
            : this(MergeDifferences(previous.Differences, differences, provider), provider) { }

        public PointInTime WithDifferences(IEnumerable<Difference> differences) => new(this, differences, new TimeProvider(this));

        private static List<Difference> MergeDifferences(IEnumerable<Difference> previouses, IEnumerable<Difference> differences, IContentProvider virtualProvider)
        {
            var merged = new List<Difference>();

            merged.AddRange(previouses.Select(p => p.WithVirtualContentProvider(virtualProvider)));
            merged.AddRange(differences.Select(d => d.WithVirtualContentProvider(virtualProvider)));

            return merged;
        }

        public static PointInTime CreateEmpty(IContentProvider? parentProvder = null) =>
            parentProvder == null ? new PointInTime() : new PointInTime(new List<Difference>(), parentProvder);
    }
}