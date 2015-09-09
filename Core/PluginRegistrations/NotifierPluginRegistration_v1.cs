using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
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
        ICrate _crate;
        string label = "Notifier Plugin Registration";
        public NotifierPluginRegistration_v1()
            : base(InitAvailableActions(), baseUrl, PluginRegistrationName)
        {
            _crate = ObjectFactory.GetInstance<ICrate>();
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
                label = "Send an Email";
                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Email Address', required: true, value: '', fieldLabel: 'Email Address' }"));
                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Friendly Name', required: true, value: '', fieldLabel: 'Friendly Name' }"));
                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Subject', required: true, value: '', fieldLabel: 'Subject' }"));
                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Body', required: true, value: '', fieldLabel: 'Body' }"));
            }
            else if (curActionTemplate.Name.Equals("Send a Text (SMS) Message", StringComparison.OrdinalIgnoreCase))
            {
                label = "Send a Text (SMS) Message";

                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Phone Number', required: true, value: '', fieldLabel: 'Phone Number' }"));
                curCrateStorage.CratesDTO.Add(_crate.Create(label, "{ name: 'Message', required: true, value: '', fieldLabel: 'Message' }"));
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
