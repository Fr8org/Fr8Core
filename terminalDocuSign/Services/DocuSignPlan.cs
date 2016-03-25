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
using terminalDocuSign.Services.New_Api;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related plans in Hub
    /// </summary>
    public class DocuSignPlan : IDocuSignPlan
    {
        private readonly IHubCommunicator _hubCommunicator;
        private readonly ICrateManager _crateManager;
        private readonly IDocuSignManager _docuSignManager;
        private readonly IDocuSignConnect _docuSignConnect;

        private readonly string DevConnectName = "(dev) Fr8 Company DocuSign integration";
        private readonly string ProdConnectName = "Fr8 Company DocuSign integration";

        public DocuSignPlan()
        {
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _docuSignManager = ObjectFactory.GetInstance<IDocuSignManager>();
            _docuSignConnect = ObjectFactory.GetInstance<IDocuSignConnect>();
            _hubCommunicator.Configure("terminalDocuSign");
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// 
        /// https://maginot.atlassian.net/wiki/display/DDW/Rework+of+DocuSign+connect+management
        /// </summary>
        public async Task CreatePlan_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            if (CreateConnect(curFr8UserId, authTokenDTO))
            {
                await DeleteExistingPlan(curFr8UserId, authTokenDTO);
                await CreateAndActivateNewPlan(curFr8UserId, authTokenDTO);
            }
        }


        //only create a connect when running on dev/production
        private bool CreateConnect(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
            {
            var authTokenDO = new AuthorizationTokenDO() { Token = authTokenDTO.Token, ExternalAccountId = authTokenDTO.ExternalAccountId };
            var config = _docuSignManager.SetUp(authTokenDO);

            string terminalUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint");
            string prodUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultProductionUrl");
            string devUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDevUrl");
            string publishUrl = "http://" + terminalUrl + "/terminals/terminalDocuSign/events";

            if (terminalUrl.Contains(devUrl))
                _docuSignConnect.CreateOrActivateConnect(config, DevConnectName, publishUrl);
            else
                if (terminalUrl.Contains(prodUrl))
                _docuSignConnect.CreateOrActivateConnect(config, ProdConnectName, publishUrl);
            else
                return false;

            return true;
        }

        private async Task DeleteExistingPlan(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            var existingPlans = (await _hubCommunicator.GetPlansByName("MonitorAllDocuSignEvents", curFr8UserId)).ToList();
            if (existingPlans.Count > 0)
            {
                try
                {
                    //search for existing MADSE plan for this DS account and deleting it

                    var plan = existingPlans.Where(
                          a => a.Plan.SubPlans.Any(b =>
                              _crateManager.GetStorage(b.Activities[0]).Where(t => t.Label == "DocuSignUserCrate")
                              .FirstOrDefault().Get<StandardPayloadDataCM>().GetValues("DocuSignUserEmail").FirstOrDefault() == authTokenDTO.ExternalAccountId)).FirstOrDefault();

                    if (plan != null)
                        await _hubCommunicator.DeletePlan(plan.Plan.Id, curFr8UserId);
                }
                catch { };
            }
                }

        private async Task CreateAndActivateNewPlan(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            var emptyMonitorPlan = new PlanEmptyDTO
            {
                Name = "MonitorAllDocuSignEvents",
                Description = "MonitorAllDocuSignEvents",
                PlanState = PlanState.Active,
                Visibility = PlanVisibility.Internal
            };

            var monitorDocusignPlan = await _hubCommunicator.CreatePlan(emptyMonitorPlan, curFr8UserId);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null, curFr8UserId);
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Prep DocuSign Events For Storage");
            var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "SaveToFr8Warehouse");
            await _hubCommunicator.CreateAndConfigureActivity(recordDocusignEventsTemplate.Id, 
                curFr8UserId, "Record DocuSign Events", 1, monitorDocusignPlan.Plan.StartingSubPlanId, false, new Guid(authTokenDTO.Id));
            var storeMTDataActivity = await _hubCommunicator.CreateAndConfigureActivity(storeMTDataTemplate.Id, 
                curFr8UserId, "Save To Fr8 Warehouse", 2, monitorDocusignPlan.Plan.StartingSubPlanId);
            SetSelectedCrates(storeMTDataActivity);
            //save this
            await _hubCommunicator.ConfigureActivity(storeMTDataActivity, curFr8UserId);
            var planDO = Mapper.Map<PlanDO>(monitorDocusignPlan.Plan);
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