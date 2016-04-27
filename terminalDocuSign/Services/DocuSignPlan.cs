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
using Utilities;
using Hub.Managers.APIManagers.Transmitters.Restful;

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
        private readonly IncidentReporter _alertReporter;

        private readonly string DevConnectName = "(dev) Fr8 Company DocuSign integration";
        private readonly string ProdConnectName = "Fr8 Company DocuSign integration";
        private readonly string TemporaryConnectName = "int-tests-Fr8";


        public DocuSignPlan()
        {
            _alertReporter = ObjectFactory.GetInstance<IncidentReporter>();
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
            string currentPlanId = await FindAndActivateExistingPlan(curFr8UserId, "MonitorAllDocuSignEvents", authTokenDTO);
            if (string.IsNullOrEmpty(currentPlanId))
                await CreateAndActivateNewMADSEPlan(curFr8UserId, authTokenDTO);
        }

        //only create a connect when running on dev/production
        public void CreateConnect(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            var authTokenDO = new AuthorizationTokenDO() { Token = authTokenDTO.Token, ExternalAccountId = authTokenDTO.ExternalAccountId };
            var config = _docuSignManager.SetUp(authTokenDO);

            string terminalUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint");
            string prodUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultProductionUrl");
            string devUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDevUrl");

            string connectName = "";
            string connectId = "";


            Console.WriteLine("Connect creation: terminalUrl = {0}", terminalUrl);
            if (!string.IsNullOrEmpty(terminalUrl))
            {
                if (terminalUrl.Contains(devUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    connectName = DevConnectName;
                    // DS doesn't accept port in url, so instead of
                    //http://dev-terminals.fr8.co:53234/terminals/terminalDocuSign/events
                    // we should use
                    //http://dev-terminaldocusign.fr8.co/terminals/terminalDocuSign/events
                    terminalUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.OverrideDevUrl");
                }
                else
                    if (terminalUrl.Contains(prodUrl, StringComparison.InvariantCultureIgnoreCase))
                    connectName = ProdConnectName;


                string publishUrl = terminalUrl + "/terminals/terminalDocuSign/events";

                Console.WriteLine("Connect creation: publishUrl = {0}", publishUrl);

                if (!string.IsNullOrEmpty(connectName))
                {
                    connectId = _docuSignConnect.CreateOrActivateConnect(config, connectName, publishUrl);
                    Console.WriteLine("Created connect named {0} pointing to {1} with id {2}", connectName, publishUrl, connectId);
                }
                else
                {
                    // terminal has a temporary url
                    var connectsInfo = _docuSignConnect.ListConnects(config);
                    var connects = connectsInfo.Where(a => a.name == TemporaryConnectName).ToList();
                    foreach (var connect in connects)
                    {
                        _docuSignConnect.DeleteConnect(config, connect.connectId);
                    }

                    connectId = _docuSignConnect.CreateConnect(config, TemporaryConnectName, publishUrl);
                    Console.WriteLine("Created connect named {0} pointing to {1} with id {2}", TemporaryConnectName, publishUrl, connectId);
                }
            }
        }

        public async void CreateOrUpdatePolling(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            DocuSignPolling polling = new DocuSignPolling();
            polling.SchedulePolling(authTokenDTO.ExternalAccountId, curFr8UserId);
        }


        private bool CheckIfSaveToFr8WarehouseConfiguredWithOldManifest(PlanDTO val)
        {
            return (_crateManager.GetStorage(val.Plan.SubPlans.ElementAt(0).Activities[1]).CrateContentsOfType<StandardConfigurationControlsCM>()
                     .First().FindByName("UpstreamCrateChooser") as UpstreamCrateChooser).SelectedCrates.Count > 1;
        }

        private async Task<string> FindAndActivateExistingPlan(string curFr8UserId, string plan_name, AuthorizationTokenDTO authTokenDTO)
        {
            try
            {
                var existingPlans = (await _hubCommunicator.GetPlansByName(plan_name, curFr8UserId, PlanVisibility.Internal)).ToList();
                if (existingPlans.Count > 0)
                {
                    //search for existing MADSE plan for this DS account and updating it

                    //1 Split existing plans in obsolete/malformed and new
                    var plans = existingPlans.GroupBy
                        (val =>
                        //first condition
                        val.Plan.SubPlans.Count() > 0 &&
                        //second condition
                        val.Plan.SubPlans.ElementAt(0).Activities.Count() > 0 &&
                        //third condtion
                        _crateManager.GetStorage(val.Plan.SubPlans.ElementAt(0).Activities[0]).Where(t => t.Label == "DocuSignUserCrate").FirstOrDefault() != null &&
                        //fourth condition -> check if SaveToFr8Warehouse configured with old manifests
                        !(plan_name == "MonitorAllDocuSignEvents" && CheckIfSaveToFr8WarehouseConfiguredWithOldManifest(val))

                      ).ToDictionary(g => g.Key, g => g.ToList());

                    //trying to find an existing plan for this DS account
                    if (plans.ContainsKey(true))
                    {
                        List<PlanDTO> newPlans = plans[true];

                        existingPlans = newPlans.Where(
                              a => a.Plan.SubPlans.Any(b =>
                                  _crateManager.GetStorage(b.Activities[0]).Where(t => t.Label == "DocuSignUserCrate")
                                  .FirstOrDefault().Get<StandardPayloadDataCM>().GetValues("DocuSignUserEmail").FirstOrDefault() == authTokenDTO.ExternalAccountId)).ToList();

                        if (existingPlans.Count > 1)
                            _alertReporter.EventManager_EventMultipleMonitorAllDocuSignEventsPlansPerAccountArePresent(authTokenDTO.ExternalAccountId);

                        var existingPlan = existingPlans.FirstOrDefault();

                        if (existingPlan != null)
                        {
                            var firstActivity = existingPlan.Plan.SubPlans.Where(a => a.Activities.Count > 0).FirstOrDefault().Activities[0];

                            if (firstActivity != null)
                            {
                                await _hubCommunicator.ApplyNewToken(firstActivity.Id, Guid.Parse(authTokenDTO.Id), curFr8UserId);
                                var existingPlanDO = Mapper.Map<PlanDO>(existingPlan.Plan);
                                await _hubCommunicator.ActivatePlan(existingPlanDO, curFr8UserId);
                                Console.WriteLine("#### Existing MADSE plan activated with planId: " + existingPlanDO.Id);
                                return existingPlan.Plan.Id.to_S();
                            }
                        }
                    }

                    //removing obsolete/malformed plans
                    if (plans.ContainsKey(false))
                    {
                        List<PlanDTO> obsoletePlans = plans[false];
                        Console.WriteLine("#### Found " + obsoletePlans.Count + " obsolete MADSE plans");
                        foreach (var obsoletePlan in obsoletePlans)
                        {
                            await _hubCommunicator.DeletePlan(obsoletePlan.Plan.Id, curFr8UserId);
                        }
                    }
                }
            }
            // if anything bad happens we would like not to create a new MADSE plan and fail loudly
            catch (Exception exc) { throw new ApplicationException("Couldn't update an existing Monitor_All_DocuSign_Events plan", exc); };

            return null;
        }

        private async Task CreateAndActivateNewMADSEPlan(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
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
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Prepare_DocuSign_Events_For_Storage");
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

            Console.WriteLine("#### New MADSE plan activated with planId: " + planDO.RootPlanNodeId);
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
                    selectedKey = MT.DocuSignEnvelope_v2.ToString(),
                    Value = ((int)MT.DocuSignEnvelope_v2).ToString(),
                    Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                    Source = existingDdlbSource
                };

                upstreamCrateChooser.SelectedCrates = new List<CrateDetails>()
                {
                    new CrateDetails { ManifestType = docusignEnvelope, Label = existingLabelDdlb }
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