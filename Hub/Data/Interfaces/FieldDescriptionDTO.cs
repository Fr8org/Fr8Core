namespace Data.Interfaces
{
    public class FieldDescriptionDTO
    {
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public string FieldType { get; set; }
        public bool IsArray { get; set; }

        public FieldDescriptionDTO()
        {
        }

        public FieldDescriptionDTO(string path, string fieldType)
        {
            Path = path;
            DisplayName = path;
            FieldType = fieldType;
        }

        public FieldDescriptionDTO(string path, string displayName, string fieldType)
        {
            Path = path;
            DisplayName = displayName;
            FieldType = fieldType;
        }
    }
}
