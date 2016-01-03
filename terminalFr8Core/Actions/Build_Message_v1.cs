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
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        protected async override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
            }

            return await AddDesignTimeFieldsSource(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await base.FollowupConfigurationResponse(curActionDO, authTokenDO);
        }

        private Crate CreateControlsCrate()
        {
            var controls = new List<ControlDefinitionDTO>()
            {
                new TextBox()
                {
                    Label = "Name",
                    Name = "Name"
                },
                new TextArea()
                {
                    Label = "Body",
                    Name = "Body"
                    ,IsReadOnly = false
                    ,Value = "test"
                    , Required = true
                },
                new DropDownList
                {
                    Name = "AvailableFields",
                    Required = true,
					Label = "Available Fields",
                    Source = new FieldSourceDTO
                    {
                        Label = "Available Fields",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }
            };

            return Crate.CreateStandardConfigurationControlsCrate("Craft a Message", controls.ToArray());
        }

        private async Task<ActionDO> AddDesignTimeFieldsSource(ActionDO curActionDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveByLabel("Available Fields");

                var upstreamFieldsAddress = await MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActionDO, "Available Fields");
                if (upstreamFieldsAddress != null)
                    updater.CrateStorage.Add(upstreamFieldsAddress);
            }

            return curActionDO;
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
    }
}