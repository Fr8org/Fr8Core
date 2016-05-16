using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Converters;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class ConvertCrates_v1 : BaseTerminalActivity
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
            { new ManifestTypeMatch(MT.DocuSignTemplate, MT.StandardFileHandle), new DocuSignTemplateToStandardFileDescriptionConversion() },
            { new ManifestTypeMatch(MT.StandardFileHandle, MT.DocuSignTemplate), new StandardFileDescriptionToDocuSignTemplateConversion() }
        };

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            //find from type
            var controlsMS = GetConfigurationControls(curActivityDO);

            var fromDropdown = (DropDownList)GetControl(controlsMS, "Available_From_Manifests", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(fromDropdown.Value))
            {
                return Error(curPayloadDTO, "No value was selected on From Manifest Type Dropdown", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var fromManifestType = Int32.Parse(fromDropdown.Value);

            //find target type 
            var toDropdown = (DropDownList)GetControl(controlsMS, "Available_To_Manifests", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(toDropdown.Value))
            {
                return Error(curPayloadDTO, "No value was selected on To Manifest Type Dropdown", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var toManifestType = Int32.Parse(toDropdown.Value);

            var convertor = ConversionMap.FirstOrDefault(c => c.Key.From == (MT)fromManifestType && c.Key.To == (MT) toManifestType).Value;

            //find user selected crate in payload
            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);
            var userSelectedFromCrate = payloadStorage.FirstOrDefault(c => c.ManifestType.Id == fromManifestType);
            if (userSelectedFromCrate == null)
            {
                return Error(curPayloadDTO, "Unable to find crate with Manifest Type : "+ fromDropdown.selectedKey, ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }

            var convertedCrate = convertor.Convert(userSelectedFromCrate);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                crateStorage.Add(convertedCrate);
            }

            return Success(curPayloadDTO);
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                crateStorage.Add(GetAvailableFromManifests());
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var manifestTypeDropdown = controlsMS.Controls.Single(x => x.Type == ControlTypes.DropDownList && x.Name == "Available_From_Manifests");

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveUsingPredicate(c => c.IsOfType<FieldDescriptionsCM>() && c.Label == "Available To Manifests");
                if (manifestTypeDropdown.Value != null)
                {
                    crateStorage.Add(GetAvailableToManifests(manifestTypeDropdown.Value));
                }
            }
            
            return curActivityDO;
        }

        private Crate GetAvailableToManifests(String manifestId)
        {
            var manifestType = (MT) Int32.Parse(manifestId);
            var toManifestList = ConversionMap.Where(c => c.Key.From == manifestType)
                .Select(c => c.Key.To)
                .Select(c => new FieldDTO(c.GetEnumDisplayName(), ((int) c).ToString(CultureInfo.InvariantCulture)));

            var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Available To Manifests", toManifestList.ToArray());
            return queryFieldsCrate;
        }

        private Crate GetAvailableFromManifests()
        {
            var toManifestList = ConversionMap
                .GroupBy(c => c.Key.From)
                .Select(c => c.Key)
                .Select(c => new FieldDTO(c.GetEnumDisplayName(), ((int)c).ToString(CultureInfo.InvariantCulture)));

            var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Available From Manifests", toManifestList.ToArray());
            return queryFieldsCrate;
        }

        private Crate CreateControlsCrate()
        {
            var infoText = new TextBlock
            {
                Value = "This activity converts data from one type of Crate to another type of Crate"
            };
            var availableFromManifests = new DropDownList
            {
                Label = "Convert upstream data from which Crate",
                Name = "Available_From_Manifests",
                Value = null,
                Events = new List<ControlEvent>{ ControlEvent.RequestConfig },
                Source = new FieldSourceDTO
                {
                    Label = "Available From Manifests",
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
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
                    ManifestType = MT.FieldDescription.GetEnumDisplayName()
                }
            };

            return PackControlsCrate(infoText, availableFromManifests, availableToManifests);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}