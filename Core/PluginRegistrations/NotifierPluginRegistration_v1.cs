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
        //private ActionNameListDTO availableActions = InitAvailableActions();//@"[{ ""ActionType"" : """" , ""Version"": ""1.0""},{ ""ActionType"" : """" , ""Version"": ""1.0""}]";
        // ActionNameListDTO availableActions = InitAvailableActions();
        public NotifierPluginRegistration_v1()
            : base(InitAvailableActions(), baseUrl)
        {
        }

        public ConfigurationSettingsDTO GetConfigurationSettings(ActionDO curAction)
        {
            if (curAction == null)
                throw new ArgumentNullException("curAction");
            if (string.IsNullOrEmpty(curAction.Name))
                throw new NullReferenceException("curAction.UserLabel");
            return InitConfigurationSettings(curAction.Name);
        }

        private ConfigurationSettingsDTO InitConfigurationSettings(string actionType)
        {
            ConfigurationSettingsDTO curConfigurationSettings = new ConfigurationSettingsDTO();
            if (actionType.Equals("Send an Email", StringComparison.OrdinalIgnoreCase))
            {
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Email Address", true, "", ""));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Friendly Name", true, "", ""));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Subject", true, "", ""));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Body", true, "", ""));
            }
            else if (actionType.Equals("Send a Text (SMS) Message", StringComparison.OrdinalIgnoreCase))
            {
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Phone Number", true, "", ""));
                curConfigurationSettings.Fields.Add(new FieldDefinitionDTO("Message", true, "", ""));
            }
            return curConfigurationSettings;
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
