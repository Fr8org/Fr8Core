using System.Text;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization
{
    public interface IEncodingStack        
    {
        Encoding Current { get; }
        void Push(Encoding encoding);
        Encoding Pop();
    }
}
