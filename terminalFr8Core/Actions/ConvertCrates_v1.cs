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
using terminalFr8Core.Converters;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class ConvertCrates_v1 : BaseTerminalAction
    {
        private class ManifestTypeMatch
        {
            public ManifestTypeMatch(MT From, MT To)
            {
                this.From = From;
                this.To = To;
            }

            public MT From { get; set; }
            public MT To { get; set; }
        }

        private static readonly Dictionary<ManifestTypeMatch, ICrateConversion> ConversionMap = new Dictionary<ManifestTypeMatch, ICrateConversion>
        {
            { new ManifestTypeMatch(MT.DocuSignTemplate, MT.StandardFileHandle), new DocuSignTemplateToStandardFileDescriptionConversion() }
        };

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActionDO, containerId);

            //find from type
            var controlsMS = GetConfigurationControls(curActionDO);

            var fromDropdown = (DropDownList)GetControl(controlsMS, "Available_From_Manifests", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(fromDropdown.Value))
            {
                return Error(curPayloadDTO, "No value was selected on From Manifest Type Dropdown", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var fromManifestType = Int32.Parse(fromDropdown.Value);

            //find target type 
            var toDropdown = (DropDownList)GetControl(controlsMS, "Available_To_Manifests", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(toDropdown.Value))
            {
                return Error(curPayloadDTO, "No value was selected on To Manifest Type Dropdown", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var toManifestType = Int32.Parse(toDropdown.Value);

            var convertor = ConversionMap.FirstOrDefault(c => c.Key.From == (MT)fromManifestType && c.Key.To == (MT) toManifestType).Value;

            //find user selected crate in payload
            var payloadStorage = Crate.GetStorage(curPayloadDTO);
            var userSelectedFromCrate = payloadStorage.FirstOrDefault(c => c.ManifestType.Id == fromManifestType);
            if (userSelectedFromCrate == null)
            {
                return Error(curPayloadDTO, "Unable to find crate with Manifest Type : "+ fromDropdown.selectedKey, ActionErrorCode.PAYLOAD_DATA_MISSING);
            }

            var convertedCrate = convertor.Convert(userSelectedFromCrate);

            using (var updater = Crate.UpdateStorage(curPayloadDTO))
            {
                updater.CrateStorage.Add(convertedCrate);
            }

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
                updater.CrateStorage.Add(GetAvailableFromManifests());
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
            var toManifestList = ConversionMap.Where(c => c.Key.From == manifestType)
                .Select(c => c.Key.To)
                .Select(c => new FieldDTO(c.GetEnumDisplayName(), ((int) c).ToString(CultureInfo.InvariantCulture)));

            var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Available To Manifests", toManifestList.ToArray());
            return queryFieldsCrate;
        }

        private Crate GetAvailableFromManifests()
        {
            var toManifestList = ConversionMap
                .GroupBy(c => c.Key.From)
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