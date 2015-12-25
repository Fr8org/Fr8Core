using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AutoMapper.Internal;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Services;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class StoreFile_v1 : BaseTerminalAction
    {
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }


        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
            }

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return curActionDO;
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);

            return Success(curPayloadDTO);
        }

        private Crate CreateControlsCrate()
        {
            var infoText = new TextBlock
            {
                Value = "This action converts data from one type of Crate to another type of Crate"
            };
            var availableFromManifests = new DropDownList
            {
                Label = "Convert upstream data from which Crate",
                Name = "Available_From_Manifests",
                Value = null,
                Events = new List<ControlEvent>{ new ControlEvent("onChange", "requestConfig") },
                Source = new FieldSourceDTO
                {
                    Label = "Available From Manifests",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var availableToManifests = new DropDownList
            {
                Label = "To which Crate:",
                Name = "Available_To_Manifests",
                Value = null,
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Source = new FieldSourceDTO
                {
                    Label = "Available To Manifests",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            return PackControlsCrate(infoText, availableFromManifests, availableToManifests);
        }
    }
}