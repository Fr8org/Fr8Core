using fr8.Infrastructure.Data.DataTransferObjects;

namespace fr8.Infrastructure.Data.Crates
{
    public interface ICrateStorageSerializer
    {
        CrateStorageDTO ConvertToDto(ICrateStorage storage);
        CrateDTO ConvertToDto(Crate crate);
        ICrateStorage ConvertFromDto(CrateStorageDTO dto);
        Crate ConvertFromDto(CrateDTO dto);
    }
}
