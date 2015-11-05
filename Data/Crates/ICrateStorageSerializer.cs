using Data.Interfaces.DataTransferObjects;

namespace Data.Crates
{
    public interface ICrateStorageSerializer
    {
        CrateStorageDTO ConvertToDto(CrateStorage storage);
        CrateDTO ConvertToDto(Crate crate);
        CrateStorage ConvertFromDto(CrateStorageDTO dto);
        Crate ConvertFromDto(CrateDTO dto);
    }
}
