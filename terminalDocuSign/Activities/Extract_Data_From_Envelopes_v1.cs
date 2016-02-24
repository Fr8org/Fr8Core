using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Extract_Data_From_Envelopes_v1 : BaseDocuSignActivity
    {
        private const string SolutionName = "Extract Data From Envelopes";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>This powerful report generator extends the capabilities of the standard DocuSign reporting tools. 
                                                Search by Recipient or Template and build powerful queries with a few mouse clicks</p>";


        private class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList FinalActivitiesList { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<img height=\"30px\" src=\"/Content/icons/web_services/DocuSign-Logo.png\">" +
                            "<p>You will be asked to select a DocuSign Template.</p>" +
                            "<p>Each time a related DocuSign Envelope is completed, we'll extract the data for you.</p>"

                });

                Controls.Add((FinalActivitiesList = new DropDownList
                {
                    Name = "FinalActionsList",
                    Required = true,
                    Label = "What would you like us to do with the data?",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableActions",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                }));
            }
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivtyDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivtyDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.AddRange(await PackSources(curActivtyDO));
            }

            return curActivtyDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First());

            //don't add child actions until a selection is made
            if (string.IsNullOrEmpty(actionUi.FinalActivitiesList.Value))
            {
                return curActivityDO;
            }

            curActivityDO.ChildNodes = new List<RouteNodeDO>();

            // Always use default template for solution
            const string firstTemplateName = "Monitor_DocuSign_Envelope_Activity";

            var firstActivity = await AddAndConfigureChildActivity(curActivityDO, firstTemplateName);
            var secondActivity = await AddAndConfigureChildActivity(curActivityDO, actionUi.FinalActivitiesList.Value, "Final activity");

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(activityDO, containerId));
        }

        private async Task<IEnumerable<ActivityTemplateDO>> FindTemplates(ActivityDO activityDO, Predicate<ActivityTemplateDO> query)
        {
            var templates = await HubCommunicator.GetActivityTemplates(activityDO, CurrentFr8UserId);
            return templates.Select(x => Mapper.Map<ActivityTemplateDO>(x)).Where(x => query(x));
        }

        private async Task<IEnumerable<Crate>> PackSources(ActivityDO activityDO)
        {
            var sources = new List<Crate>();

            var templates = await HubCommunicator.GetActivityTemplates(activityDO, ActivityCategory.Forwarders, CurrentFr8UserId);
            sources.Add(
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableActions",
                    templates.Select(x => new FieldDTO(x.Label, x.Id.ToString(), AvailabilityType.Configuration)).ToArray()
                )
            );

            return sources;
        }
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainExtractData"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution work with DocuSign envelops"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonce("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonce("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}