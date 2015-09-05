using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PluginRegistrations
{
    public class NotifierPluginRegistration_v1 : BasePluginRegistration
    {
        public const string baseUrl = "Notifier.BaseUrl";

        public const string PluginRegistrationName = "Notifier";

        //private ActionNameListDTO availableActions = InitAvailableActions();//@"[{ ""ActionType"" : """" , ""Version"": ""1.0""},{ ""ActionType"" : """" , ""Version"": ""1.0""}]";
        // ActionNameListDTO availableActions = InitAvailableActions();
        public NotifierPluginRegistration_v1()
            : base(InitAvailableActions(), baseUrl, PluginRegistrationName)
        {
        }

        public string GetConfigurationSettings(ActionTemplateDO curActionTemplate)
        {
            if (curActionTemplate == null)
                throw new ArgumentNullException("curActionTemplate");

            if (String.IsNullOrEmpty(curActionTemplate.ActionType))
                throw new ArgumentNullException("curActionTemplate.ActionType");

            ConfigurationSettingsDTO curConfigurationSettings = new ConfigurationSettingsDTO();

            if (curActionTemplate.ActionType.Equals("Send an Email", StringComparison.OrdinalIgnoreCase))
            {
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Email Address", true, "", "Email Address"));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Friendly Name", true, "", "Friendly Name"));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Subject", true, "", "Subject"));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Body", true, "", "Body"));
            }
            else if (curActionTemplate.ActionType.Equals("Send a Text (SMS) Message", StringComparison.OrdinalIgnoreCase))
            {
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Phone Number", true, "", "Phone Number"));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Message", true, "", "Message"));
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(curConfigurationSettings);
        }

        public List<string> GetFieldMappingTargets(string curActionName, string ConfigUIData)
        {
            return null;
        }

        private static ActionNameListDTO InitAvailableActions()
        {
            ActionNameListDTO curActionNameList = new ActionNameListDTO();
            curActionNameList.ActionNames.Add(new ActionNameDTO
            {
                ActionType = "Send an Email",
                Version = "1.0"
            });
            curActionNameList.ActionNames.Add(new ActionNameDTO
            {
                ActionType = "Send a Text (SMS) Message",
                Version = "1.0"
            });
            return curActionNameList;
        }
    }
}
