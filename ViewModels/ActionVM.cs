namespace Web.ViewModels
{
	public class ActionVM: ActionVMBase
	{
	    public string ActionType { get; set; }

        public string ConfigurationSettings { get; set; }

        public string FieldMappingSettings { get; set; }

        public string ParentPluginRegistration { get; set; }
	}
}