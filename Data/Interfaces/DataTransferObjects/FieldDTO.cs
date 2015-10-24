namespace Data.Interfaces.DataTransferObjects
{
    public class FieldDTO
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public FieldDTO()
        {
        }

        public FieldDTO(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
