using Data.Interfaces.DataTransferObjects;

namespace Data.Crates
{
    public interface ICrateStorageSerializer
    {
        CrateStorageDTO ConvertToProxy(CrateStorage storage);
        CrateDTO ConvertToProxy(Crate crate);
        CrateStorage ConvertFromProxy(CrateStorageDTO dto);
        Crate ConvertFromProxy(CrateDTO dto);
    }
}
