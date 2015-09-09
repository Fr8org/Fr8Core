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

            if (String.IsNullOrEmpty(curActionTemplate.Name))
                throw new ArgumentNullException("curActionTemplate.ActionType");

            CrateStorageDTO curCrateStorage = new CrateStorageDTO();

            if (curActionTemplate.Name.Equals("Send an Email", StringComparison.OrdinalIgnoreCase))
            {
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Email Address", true, "", "Email Address"));
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Friendly Name", true, "", "Friendly Name"));
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Subject", true, "", "Subject"));
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Body", true, "", "Body"));
            }
            else if (curActionTemplate.Name.Equals("Send a Text (SMS) Message", StringComparison.OrdinalIgnoreCase))
            {
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Phone Number", true, "", "Phone Number"));
                curCrateStorage.Fields.Add(new FieldDefinitionDTO("Message", true, "", "Message"));
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(curCrateStorage);
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
                Name = "Send an Email",
                Version = "1.0"
            });
            curActionNameList.ActionNames.Add(new ActionNameDTO
            {
                Name = "Send a Text (SMS) Message",
                Version = "1.0"
            });
            return curActionNameList;
        }
    }
}
