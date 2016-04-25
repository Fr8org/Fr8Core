using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

using Hub.Managers;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{
    public class MapFields_v1 : BaseTerminalActivity
    {
        public MapFields_v1() : base("terminalFr8Core.MapFields v1")
        {
            
        }
        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var processPayload = await GetPayload(activityDO, containerId);

            var curControlsMS = CrateManager.GetStorage(activityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (curControlsMS == null)
            {
                return Error(processPayload, "No controls crate found.");
            }

            var curMappingControl = curControlsMS.Controls.FirstOrDefault(x => x.Name == "Selected_Mapping");

            if (curMappingControl == null || string.IsNullOrEmpty(curMappingControl.Value))
            {
                return Error(processPayload, "No Selected_Mapping control found.");
            }

            var mappedFields = JsonConvert.DeserializeObject<List<FieldDTO>>(curMappingControl.Value);
            mappedFields = mappedFields.Where(x => x.Key != null && x.Value != null).ToList();
            var storage = CrateManager.FromDto(processPayload.CrateStorage);

            IEnumerable<FieldDTO> processedMappedFields;
            try
            {
                processedMappedFields = mappedFields.Select(a => { return new FieldDTO(a.Value, ExtractPayloadFieldValue(storage, a.Key, activityDO)); });

                using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => processPayload.CrateStorage))
                {
                    crateStorage.Add(Data.Crates.Crate.FromContent("MappedFields", new StandardPayloadDataCM(processedMappedFields)));
                    return Success(processPayload);
                }
            }
            catch (ApplicationException exception)
            {
                //in case of problem with extract payload field values raise and Error alert to the user
                return Error(processPayload, exception.Message, null, "Map Fields", "Fr8Core");
            }
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActivityDO> Configure(ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(activityDO, ConfigurationEvaluator, authTokenDO);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private void AddMappingPane(ICrateStorage storage)
        {
            var mappingPane = new MappingPane()
            {
                Name = "Selected_Mapping",
                Required = true
            };

            AddControl(storage, mappingPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(
            ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //Filter the upstream fields by Availability flag as this action takes the run time data (left DDLBs) to the fidles (right DDLBs)
            var curUpstreamFieldsTask = GetDesignTimeFields(curActivityDO, CrateDirection.Upstream);

            //Get all the downstream fields to be mapped (right DDLBs)
            var curDownstreamFieldsTask = GetDesignTimeFields(curActivityDO, CrateDirection.Downstream);

            var curUpstreamFields = (await curUpstreamFieldsTask)
                .Fields
                .Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();

            var curDownstreamFields = (await curDownstreamFieldsTask)
                .Fields
                .Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();


            if (!(NeedsConfiguration(curActivityDO, curUpstreamFields, curDownstreamFields)))
            {
                return curActivityDO;
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            var downstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Downstream Terminal-Provided Fields", curDownstreamFields.ToList().Select(a => { a.Availability = AvailabilityType.Configuration; return a; }).ToArray());
            var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields.ToList().Select(a => { a.Availability = AvailabilityType.Configuration; return a; }).ToArray());

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
                {
                    AddErrorTextBlock(crateStorage);
                }
                else
                {
                    AddInitialTextBlock(crateStorage);
                    AddMappingPane(crateStorage);
                }

                crateStorage.Add(downstreamFieldsCrate);
                crateStorage.Add(upstreamFieldsCrate);
            }

            return curActivityDO;
        }

        private void AddInitialTextBlock(ICrateStorage storage)
        {
            var textBlock = new TextBlock()
            {
                Name = "InfoBlock",
                Value = "When this plan is executed, the values found in the fields on the left will be used for the fields on the right",
                CssClass = "well well-lg"
            };

            AddControl(storage, textBlock);
        }

        private void AddErrorTextBlock(ICrateStorage storage)
        {
            var textBlock = GenerateTextBlock("Attention",
                "In order to work this Action needs upstream and downstream Actions configured",
                "well well-lg", "MapFieldsErrorMessage");
            AddControl(storage, textBlock);
        }

        /// <summary>
        /// Check if initial configuration was requested.
        /// </summary>
        private bool NeedsConfiguration(ActivityDO curAction, FieldDTO[] curUpstreamFields, FieldDTO[] curDownstreamFields)
        {
            ICrateStorage storage = storage = CrateManager.GetStorage(curAction.CrateStorage);

            var upStreamFields = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Terminal-Provided Fields").FirstOrDefault();
            var downStreamFields = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Downstream Terminal-Provided Fields").FirstOrDefault();

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
                bool upstreamMatch = curUpstreamFields.Where(a => upStreamFields.Fields.Any(b => b.Key == a.Key)).Count() == curUpstreamFields.Count();
                bool downstreamMatch = curDownstreamFields.Where(a => downStreamFields.Fields.Any(b => b.Key == a.Key)).Count() == curDownstreamFields.Count();

                return !(upstreamMatch & downstreamMatch);
            }
        }


        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return ConfigurationRequestType.Initial;

        }
    }
}