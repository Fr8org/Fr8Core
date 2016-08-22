using System;
using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface ITrackingChangesRepository
    {
        Type EntityType { get; }

        void TrackAdds(IEnumerable<object> entities);
        void TrackDeletes(IEnumerable<object> entities);
        void TrackUpdates(IEnumerable<object> entities);

        void SaveChanges();
    }
}
