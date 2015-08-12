namespace Data.Interfaces.DataTransferObjects
{
    // TODO: Do we need this class? There is one in Web project.
    public class ActionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UserLabel { get; set; }

        public string ActionType { get; set; }

        public int? ActionListId { get; set; }

        public string ConfigurationSettings { get; set; }

        public string FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }
    }
}
