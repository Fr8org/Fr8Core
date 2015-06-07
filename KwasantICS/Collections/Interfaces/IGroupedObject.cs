using System;

namespace KwasantICS.Collections.Interfaces
{
    public interface IGroupedObject<TGroup>
    {
        event EventHandler<ObjectEventArgs<TGroup, TGroup>> GroupChanged;
        TGroup Group { get; set; }
    }
}
