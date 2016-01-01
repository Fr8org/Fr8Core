using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Hub.Managers;
using Data.Interfaces.Manifests;
using Data.Control;
using Data.States;

namespace terminalFr8Core.Actions
{
    public class Build_Message_v1 : BaseTerminalAction
    {
        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            

            var controlsMS = Crate.GetStorage(curActionDO.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var curMergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActionDO, "Available Run-Time Objects");
            FieldDTO[] curSelectedFields = curMergedUpstreamRunTimeObjects.Content.Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

            if (controlsMS == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }
            string text = "";
            foreach (var control in controlsMS.Controls)
            {
                if (control.Type == "TextArea")
                {
                    text = control.Value;
                }
            }
            string result = text;
            for (int i = 0; i <= text.Length; i++)
            {
                if (!text.Contains("[") || !text.Contains("]"))
                {
                    break;
                }
                string str = text.Split('[', ']')[1];
                for (int j = 0; j < curSelectedFields.Count(); j++)
                {
                    if (curSelectedFields[j].Key.ToString() == str)
                    {
                        str = "[" + str + "]";
                        result = result.Replace(str, curSelectedFields[j].Value.ToString() + " ");
                        text = text.Replace(str, "");
                        break;
                    }

                }
                Console.WriteLine(str);

            }

            var curProcessPayload = await GetPayload(curActionDO, containerId);

            result = result.Replace("<p>", String.Empty).Replace("</p>", " ");

            
            List<FieldDTO> newFields = new List<FieldDTO>();
            FieldDTO _field = new FieldDTO();
            _field.Key = "message";
            _field.Value = result;
            newFields.Add(_field);

            //var messageBody = JsonConvert.DeserializeObject<List<FieldDTO>>(result);

            using (var updater = Crate.UpdateStorage(curProcessPayload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("MessageBody", new StandardPayloadDataCM(newFields)));
            }

            return curProcessPayload;

        }


        protected async override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var textBlock = new TextBlock()
            {
                Label = "Create a Message",
                Name = "CreateaMessage"
            };
            var name = new TextBox()
            {
                Label = "Name:",
                Name = "Name",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };

            var messageBody = new TextArea()
            {
                Label = "Body:",
                Name = "Body",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            //var curUpstreamFields =
            //    (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream))
            //    .Fields
            //    .ToArray();

            //var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);

            var curMergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActionDO, "Available Run-Time Objects");
            //added focusConfig in PaneConfigureAction.ts
            var fieldSelectObjectTypes = new DropDownList()
            {
                Label = "Available Fields",
                Name = "Available Fields",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "focusConfig") },
                Source = new FieldSourceDTO
                {
                    Label = curMergedUpstreamRunTimeObjects.Label,
                    ManifestType = curMergedUpstreamRunTimeObjects.ManifestType.Type
                }
            };

            var curConfigurationControlsCrate = PackControlsCrate(textBlock, name, messageBody, fieldSelectObjectTypes);


            FieldDTO[] curSelectedFields = curMergedUpstreamRunTimeObjects.Content.Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

            var curSelectedObjectType = Crate.CreateDesignTimeFieldsCrate("SelectedObjectTypes", curSelectedFields);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                if (curActionDO.CrateStorage == null)
                {
                    updater.CrateStorage.Clear();
                    updater.CrateStorage.Add(curConfigurationControlsCrate);
                    updater.CrateStorage.Add(curMergedUpstreamRunTimeObjects);
                    updater.CrateStorage.Add(curSelectedObjectType);
                }
            }

            return curActionDO;
        }


        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        
    }
}