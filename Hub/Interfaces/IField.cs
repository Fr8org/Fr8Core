using Fr8.Infrastructure.Data.DataTransferObjects;


namespace Hub.Interfaces
{
    public interface IField
    {
        bool Exists(FieldValidationDTO data);
    }
}