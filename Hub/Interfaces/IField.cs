using Fr8Data.DataTransferObjects;


namespace Hub.Interfaces
{
    public interface IField
    {
        bool Exists(FieldValidationDTO data);
    }
}