using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using StructureMap;
using terminalDocuSign.Interfaces;
using TerminalBase.Infrastructure;
using Data.Constants;

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related plans in Hub
    /// </summary>
    public class DocuSignPlan : IDocuSignPlan
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly IHubCommunicator _hubCommunicator;
        private readonly ICrateManager _crateManager;

        

        public DocuSignPlan()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _hubCommunicator.Configure("terminalDocuSign");
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        public async Task CreatePlan_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            var existingPlans = (await _hubCommunicator.GetPlansByName("MonitorAllDocuSignEvents", curFr8UserId)).ToList();

            existingPlans = existingPlans.Where(r => r.Plan.Tag == ("docusign-auto-monitor-plan-" + curFr8UserId)).ToList();
            if (existingPlans.Any())
            {
                //hmmmm which one belongs to us?
                //lets assume there will be only single plan
                var existingPlan = existingPlans.Single();
                if (existingPlan.Plan.PlanState != PlanState.Active)
                {
                    var existingPlanDO = Mapper.Map<PlanDO>(existingPlan);
                    await _hubCommunicator.ActivatePlan(existingPlanDO, curFr8UserId);
                }
                return;
            }
            //first check if this exists
            var emptyMonitorPlan = new PlanEmptyDTO
            {
                Name = "MonitorAllDocuSignEvents",
                Description = "MonitorAllDocuSignEvents",
                PlanState = PlanState.Active,
                Tag = "docusign-auto-monitor-plan-" + curFr8UserId,
                Visibility = PlanVisibility.Internal
            };
            var monitorDocusignPlan = await _hubCommunicator.CreatePlan(emptyMonitorPlan, curFr8UserId);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null, curFr8UserId);
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Record_DocuSign_Events");
            var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "SaveToFr8Warehouse");
            await _hubCommunicator.CreateAndConfigureActivity(recordDocusignEventsTemplate.Id, 
                curFr8UserId, "Record DocuSign Events", 1, monitorDocusignPlan.Plan.StartingSubPlanId, false, new Guid(authTokenDTO.Id));
            var storeMTDataActivity = await _hubCommunicator.CreateAndConfigureActivity(storeMTDataTemplate.Id, 
                curFr8UserId, "Save To Fr8 Warehouse", 2, monitorDocusignPlan.Plan.StartingSubPlanId);
            SetSelectedCrates(storeMTDataActivity);
            //save this
            await _hubCommunicator.ConfigureActivity(storeMTDataActivity, curFr8UserId);
            var planDO = Mapper.Map<PlanDO>(monitorDocusignPlan);
            await _hubCommunicator.ActivatePlan(planDO, curFr8UserId);
        }

        private void SetSelectedCrates(ActivityDTO storeMTDataActivity)
        {
            using (var crateStorage = _crateManager.UpdateStorage(() => storeMTDataActivity.CrateStorage))
            {
                var configControlCM = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First();

                var upstreamCrateChooser = (UpstreamCrateChooser)configControlCM.FindByName("UpstreamCrateChooser");
                var existingDdlbSource = upstreamCrateChooser.SelectedCrates[0].ManifestType.Source;
                var existingLabelDdlb = upstreamCrateChooser.SelectedCrates[0].Label;
                var docusignEnvelope = new DropDownList
                {
                    selectedKey = MT.DocuSignEnvelope.ToString(),
                    Value = ((int)MT.DocuSignEnvelope).ToString(),
                    Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                    Source = existingDdlbSource
                };
                var docusignEvent = new DropDownList
                {
                    selectedKey = MT.DocuSignEvent.ToString(),
                    Value = ((int)MT.DocuSignEvent).ToString(),
                    Name = "UpstreamCrateChooser_mnfst_dropdown_1",
                    Source = existingDdlbSource
                };
                var docusignRecipient = new DropDownList
                {
                    selectedKey = MT.DocuSignRecipient.ToString(),
                    Value = ((int)MT.DocuSignRecipient).ToString(),
                    Name = "UpstreamCrateChooser_mnfst_dropdown_2",
                    Source = existingDdlbSource
                };

                upstreamCrateChooser.SelectedCrates = new List<CrateDetails>()
                {
                    new CrateDetails { ManifestType = docusignEnvelope, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = docusignEvent, Label = existingLabelDdlb },
                    new CrateDetails { ManifestType = docusignRecipient, Label = existingLabelDdlb }
                };
            }
        }

        private ActivityTemplateDTO GetActivityTemplate(IEnumerable<ActivityTemplateDTO> activityList, string activityTemplateName)
        {
            var template = activityList.FirstOrDefault(x => x.Name == activityTemplateName);
            if (template == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            return template;
        }
    }
}