using KwasantICS.DDay.iCal.Interfaces.General;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();        
    }
}
