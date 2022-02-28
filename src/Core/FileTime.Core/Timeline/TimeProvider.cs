using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class TimeProvider : ContentProviderBase<TimeProvider>
    {
        private readonly PointInTime _pointInTime;

        public TimeProvider(PointInTime pointInTime) : base("time", "time2://", false)
        {
            _pointInTime = pointInTime;
        }

        public override Task<bool> CanHandlePath(string path)
        {
            throw new NotImplementedException();
        }

        public override Task<IContainer> CreateContainerAsync(string name)
        {
            throw new NotImplementedException();
        }

        public override Task<IElement> CreateElementAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}