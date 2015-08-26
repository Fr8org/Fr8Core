namespace Data.Interfaces.DataTransferObjects
{
    public class ActionDesignDTO : ActionDTOBase
    {
        public string UserLabel { get; set; }

        public string ActionType { get; set; }

        public int? ActionListId { get; set; }

        public string ConfigurationSettings { get; set; }

        public FieldMappingSettingsDTO FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }

	    public string DocuSignTemplateId { get; set; }

    }
}
