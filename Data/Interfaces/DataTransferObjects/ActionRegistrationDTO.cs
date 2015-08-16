namespace Data.Interfaces.DataTransferObjects
{
    public class ActionRegistrationDTO
    {
        public int Id { get; set; }

        public string ActionType { get; set; }

        public string Version { get; set; }

        public string ParentPluginRegistration { get; set; }
    }
}
