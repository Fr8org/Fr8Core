using System.IO;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization
{
    public interface IStringSerializer :
        ISerializer
    {
        string SerializeToString(object obj);
        object Deserialize(TextReader tr);
    }
}
