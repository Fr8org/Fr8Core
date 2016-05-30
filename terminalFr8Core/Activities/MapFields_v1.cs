using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class MapFields_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "MapFields",
            Label = "Map Fields",
            Category = ActivityCategory.Processors,
            Terminal = TerminalData.TerminalDTO,
            Tags = $"{Tags.AggressiveReload},{Tags.Internal}",
            Version = "1",
            MinPaneWidth = 380,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public MapFields_v1(ICrateManager crateManager)
            : base(false, crateManager)
        {

        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private void AddMappingPane()
        {
            var mappingPane = new MappingPane()
            {
                Name = "Selected_Mapping",
                Required = true
            };

            AddControl(mappingPane);
        }

        private void AddInitialTextBlock()
        {
            var textBlock = new TextBlock()
            {
                Name = "InfoBlock",
                Value = "When this plan is executed, the values found in the fields on the left will be used for the fields on the right",
                CssClass = "well well-lg"
            };

            AddControl(textBlock);
        }

        private void AddErrorTextBlock()
        {
            var textBlock = ControlHelper.GenerateTextBlock("Attention",
                "In order to work this Action needs upstream and downstream Actions configured",
                "well well-lg", "MapFieldsErrorMessage");
            AddControl(textBlock);
        }

        /// <summary>
        /// Check if initial configuration was requested.
        /// </summary>
        private bool NeedsConfiguration(FieldDTO[] curUpstreamFields, FieldDTO[] curDownstreamFields)
        {
            var upStreamFields = Storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Terminal-Provided Fields").FirstOrDefault();
            var downStreamFields = Storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Downstream Terminal-Provided Fields").FirstOrDefault();

            if (upStreamFields == null
                || upStreamFields.Fields == null
                || upStreamFields.Fields.Count == 0
                || downStreamFields == null
                || downStreamFields.Fields == null
                || downStreamFields.Fields.Count == 0)
            {

                return true;
            }
            else
            {
                // true if current up/downstream fields don't match saved up/downstream fields
                bool upstreamMatch = curUpstreamFields.Count(a => upStreamFields.Fields.Any(b => b.Key == a.Key)) == curUpstreamFields.Count();
                bool downstreamMatch = curDownstreamFields.Count(a => downStreamFields.Fields.Any(b => b.Key == a.Key)) == curDownstreamFields.Count();

                return !(upstreamMatch & downstreamMatch);
            }
        }

        public override async Task Run()
        {
            var curMappingControl = GetControl<MappingPane>("Selected_Mapping");
            if (string.IsNullOrEmpty(curMappingControl?.Value))
            {
                RaiseError("No Selected_Mapping control found.");
                return;
            }

            var mappedFields = JsonConvert.DeserializeObject<List<FieldDTO>>(curMappingControl.Value);
            mappedFields = mappedFields.Where(x => x.Key != null && x.Value != null).ToList();
            try
            {
                //var processedMappedFields = mappedFields.Select(a => new FieldDTO(a.Value, ExtractPayloadFieldValue(a.Key)));
                //Payload.Add(Crate.FromContent("MappedFields", new StandardPayloadDataCM(processedMappedFields)));
                Success();
            }
            catch (ApplicationException exception)
            {
                //in case of problem with extract payload field values raise and Error alert to the user
                RaiseError(exception.Message, null, "Map Fields", "Fr8Core");
            }
        }

        public override async Task Initialize()
        {
            //Filter the upstream fields by Availability flag as this action takes the run time data (left DDLBs) to the fidles (right DDLBs)
            var curUpstreamFieldsTask = GetDesignTimeFields(CrateDirection.Upstream);

            //Get all the downstream fields to be mapped (right DDLBs)
            var curDownstreamFieldsTask = GetDesignTimeFields(CrateDirection.Downstream);

            var curUpstreamFields = (await curUpstreamFieldsTask)
                .Fields
                .Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();

            var curDownstreamFields = (await curDownstreamFieldsTask)
                .Fields
                .Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();


            if (!(NeedsConfiguration(curUpstreamFields, curDownstreamFields)))
            {
                return;
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            var downstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Downstream Terminal-Provided Fields", curDownstreamFields.ToList().Select(a => { a.Availability = AvailabilityType.Configuration; return a; }).ToArray());
            var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields.ToList().Select(a => { a.Availability = AvailabilityType.Configuration; return a; }).ToArray());

            Storage.Clear();
            if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
            {
                AddErrorTextBlock();
            }
            else
            {
                AddInitialTextBlock();
                AddMappingPane();
            }

            Storage.Add(downstreamFieldsCrate);
            Storage.Add(upstreamFieldsCrate);

        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}