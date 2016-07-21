using Fr8Data.DataTransferObjects;

namespace Fr8Data.Crates
{
    public interface ICrateStorageSerializer
    {
        CrateStorageDTO ConvertToDto(ICrateStorage storage);
        CrateDTO ConvertToDto(Crate crate);
        ICrateStorage ConvertFromDto(CrateStorageDTO dto);
        Crate ConvertFromDto(CrateDTO dto);
    }
}
