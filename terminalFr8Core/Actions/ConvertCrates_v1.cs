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
using Utilities;

namespace terminalFr8Core.Actions
{
    public class ConvertCrates_v1 : BaseTerminalAction
    {
        private static readonly Dictionary<MT, MT> ConversionMap = new Dictionary<MT, MT>
        {
            { MT.DocuSignTemplate, MT.StandardFileHandle }
        };

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            
            return Success(curPayloadDTO);
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
                updater.CrateStorage.Add(GetUpstreamAvailableFromManifests());
            }

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_From_Manifests");

            
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveUsingPredicate(c => c.IsOfType<StandardDesignTimeFieldsCM>() && c.Label == "Available From Manifests");
                updater.CrateStorage.RemoveUsingPredicate(c => c.IsOfType<StandardDesignTimeFieldsCM>() && c.Label == "Available To Manifests");
                if (manifestTypeDropdown.Value != null)
                {
                    updater.CrateStorage.Add(GetAvailableToManifests(manifestTypeDropdown.Value));
                }
            }
            
            return curActionDO;
        }

        private Crate GetAvailableToManifests(String manifestId)
        {
            var manifestType = (MT) Int32.Parse(manifestId);
            var toManifestList = ConversionMap.Where(c => c.Key == manifestType)
                .Select(c => c.Value)
                .Select(c => new FieldDTO(c.GetEnumDisplayName(), ((int) c).ToString(CultureInfo.InvariantCulture)));

            var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Available To Manifests", toManifestList.ToArray());
            return queryFieldsCrate;
        }

        private Crate GetUpstreamAvailableFromManifests()
        {
            
            var toManifestList = ConversionMap
                .GroupBy(c => c.Key)
                .Select(c => c.Key)
                .Select(c => new FieldDTO(c.GetEnumDisplayName(), ((int)c).ToString(CultureInfo.InvariantCulture)));

            var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Available From Manifests", toManifestList.ToArray());
            return queryFieldsCrate;
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
    }
}