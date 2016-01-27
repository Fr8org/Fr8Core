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
    /// Service to create DocuSign related routes in Hub
    /// </summary>
    public class DocuSignRoute : IDocuSignRoute
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly IHubCommunicator _hubCommunicator;
        private readonly ICrateManager _crateManager;

        

        public DocuSignRoute()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        public async Task CreateRoute_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            var existingRoutes = (await _hubCommunicator.GetPlansByName("MonitorAllDocuSignEvents", curFr8UserId)).ToList();
            existingRoutes = existingRoutes.Where(r => r.Tag == "docusign-auto-monitor-plan-"+authTokenDTO.ExternalAccountId).ToList();
            if (existingRoutes.Any())
            {
                //hmmmm which one belongs to us?
                //lets assume there will be only single plan
                var existingRoute = existingRoutes.Single();
                if (existingRoute.RouteState != RouteState.Active)
                {
                    var existingRouteDO = Mapper.Map<PlanDO>(existingRoute);
                    await _hubCommunicator.ActivatePlan(existingRouteDO, curFr8UserId);
                }
                return;
            }
            //first check if this exists
            var emptyMonitorRoute = new RouteEmptyDTO
            {
                Name = "MonitorAllDocuSignEvents",
                Description = "MonitorAllDocuSignEvents",
                RouteState = RouteState.Active,
                Tag = "docusign-auto-monitor-route-"+authTokenDTO.ExternalAccountId
            };
            var monitorDocusignRoute = await _hubCommunicator.CreatePlan(emptyMonitorRoute, curFr8UserId);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null, curFr8UserId);
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Record_DocuSign_Events");
            var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "SaveToFr8Warehouse");
            await _hubCommunicator.CreateAndConfigureActivity(recordDocusignEventsTemplate.Id, "Record_DocuSign_Events",
                curFr8UserId, "Record DocuSign Events", monitorDocusignRoute.StartingSubrouteId, false, new Guid(authTokenDTO.Id));
            var storeMTDataActivity = await _hubCommunicator.CreateAndConfigureActivity(storeMTDataTemplate.Id, "Save To Fr8 Warehouse",
                curFr8UserId, "Save To Fr8 Warehouse", monitorDocusignRoute.StartingSubrouteId);
            SetSelectedCrates(storeMTDataActivity);
            //save this
            await _hubCommunicator.ConfigureActivity(storeMTDataActivity, curFr8UserId);
            var planDO = Mapper.Map<PlanDO>(monitorDocusignRoute);
            await _hubCommunicator.ActivatePlan(planDO, curFr8UserId);
        }

        private void SetSelectedCrates(ActivityDTO storeMTDataActivity)
        {
            using (var updater = _crateManager.UpdateStorage(() => storeMTDataActivity.CrateStorage))
            {
                var configControlCM = updater.CrateStorage
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

        private PlanDO GetExistingPlan(IUnitOfWork uow, string routeName, string fr8AccountEmail)
        {
            if (uow.RouteRepository.GetQuery().Any(existingRoute =>
                existingRoute.Name.Equals(routeName) &&
                existingRoute.Fr8Account.Email.Equals(fr8AccountEmail)))
            {
                return uow.RouteRepository.GetQuery().First(existingRoute =>
                    existingRoute.Name.Equals(routeName) &&
                    existingRoute.Fr8Account.Email.Equals(fr8AccountEmail));
            }

            return null;
        }
    }
}