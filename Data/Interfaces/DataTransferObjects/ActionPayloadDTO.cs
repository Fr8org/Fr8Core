namespace Data.Interfaces.DataTransferObjects
{
    public class ActionPayloadDTO : ActionDTOBase
    {
        public string UserLabel { get; set; }

        public string ActionType { get; set; }

        public int? ActionListId { get; set; }

        public string ConfigurationSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

        public string PayloadMappings { get; set; }

        public string EnvelopeId { get; set; }
    }
}
