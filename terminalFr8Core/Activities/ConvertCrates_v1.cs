using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.Control;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.Manifests;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities;
using terminalFr8Core.Converters;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{
    public class ConvertCrates_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "ConvertCrates",
            Label = "Convert Crates",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            Tags = Tags.Internal,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
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

        public ConvertCrates_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task Run()
        {
            //find from type
            var fromDropdown = GetControl<DropDownList>("Available_From_Manifests");
            if (string.IsNullOrEmpty(fromDropdown.Value))
            {
                RaiseError("No value was selected on From Manifest Type Dropdown", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }
            var fromManifestType = int.Parse(fromDropdown.Value);
            //find target type 
            var toDropdown = GetControl<DropDownList>("Available_To_Manifests");
            if (string.IsNullOrEmpty(toDropdown.Value))
            {
                RaiseError("No value was selected on To Manifest Type Dropdown", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }
            var toManifestType = Int32.Parse(toDropdown.Value);
            var convertor = ConversionMap.FirstOrDefault(c => c.Key.From == (MT)fromManifestType && c.Key.To == (MT)toManifestType).Value;
            //find user selected crate in payload
            var userSelectedFromCrate = Payload.FirstOrDefault(c => c.ManifestType.Id == fromManifestType);
            if (userSelectedFromCrate == null)
            {
                RaiseError("Unable to find crate with Manifest Type : " + fromDropdown.selectedKey, ActivityErrorCode.PAYLOAD_DATA_MISSING);
                return;
            }
            var convertedCrate = convertor.Convert(userSelectedFromCrate);
            Payload.Add(convertedCrate);
            Success();
        }

        public override Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            Storage.Add(GetAvailableFromManifests());
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            var manifestTypeDropdown = GetControl<DropDownList>("Available_From_Manifests");
            Storage.RemoveUsingPredicate(c => c.IsOfType<FieldDescriptionsCM>() && c.Label == "Available To Manifests");
            if (manifestTypeDropdown.Value != null)
            {
                Storage.Add(GetAvailableToManifests(manifestTypeDropdown.Value));
            }

            await Task.Yield();
        }
    }
}