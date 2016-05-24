using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using StructureMap;
using terminalDocuSign.Interfaces;
using TerminalBase.Infrastructure;
using terminalDocuSign.Services.New_Api;
using Utilities.Configuration.Azure;
using Utilities;
using Utilities.Logging;

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
        private readonly string DemoConnectName = "(demo) Fr8 Company DocuSign integration";
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
            Logger.LogInfo($"Create MADSE called {curFr8UserId}");
            string currentPlanId = await FindAndActivateExistingPlan(curFr8UserId, "MonitorAllDocuSignEvents", authTokenDTO);
            if (string.IsNullOrEmpty(currentPlanId))
                await CreateAndActivateNewMADSEPlan(curFr8UserId, authTokenDTO);
        }

        //only create a connect when running on dev/production
        public void CreateConnect(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            Logger.LogInfo($"CreateConnect called {curFr8UserId}");
            var authTokenDO = new AuthorizationTokenDO() { Token = authTokenDTO.Token, ExternalAccountId = authTokenDTO.ExternalAccountId };
            var config = _docuSignManager.SetUp(authTokenDO);

            string terminalUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint");
            string prodUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultProductionUrl");
            string devUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDevUrl");
            string demoUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDemoUrl");

            string connectName = "";
            string connectId = "";

            Logger.LogInfo($"CreateConnect terminalUrl {terminalUrl}", DocuSignManager.DocusignTerminalName);
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
                {
                    connectName = ProdConnectName;
                }
                else
                    if (terminalUrl.Contains(demoUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    connectName = DemoConnectName;
                }
                else
                {
                    Logger.LogInfo($"Unable to set connectName from {terminalUrl}", DocuSignManager.DocusignTerminalName);
                }

                string publishUrl = terminalUrl + "/terminals/terminalDocuSign/events";

                Logger.LogInfo("Connect creation: publishUrl = {0}", DocuSignManager.DocusignTerminalName);

                if (!string.IsNullOrEmpty(connectName))
                {
                    connectId = _docuSignConnect.CreateOrActivateConnect(config, connectName, publishUrl);
                    Logger.LogInfo($"Created connect named {connectName} pointing to {publishUrl} with id {connectId}", DocuSignManager.DocusignTerminalName);
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
                    Logger.LogInfo($"Created connect named {TemporaryConnectName} pointing to {publishUrl} with id {connectId}", DocuSignManager.DocusignTerminalName);
                }
            }
            else
            {
                Logger.LogInfo($"terminalUrl is empty, no work has been done in DocuSignPlan.CreateConnect: prodUrl -> {prodUrl}, devUrl -> {devUrl}, demoUrl -> {demoUrl}");
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
                                await _hubCommunicator.RunPlan(existingPlan.Plan.Id, new List<CrateDTO>(),  curFr8UserId);
                                Logger.LogInfo($"#### Existing MADSE plan activated with planId: {existingPlan.Plan.Id}", DocuSignManager.DocusignTerminalName);
                                return existingPlan.Plan.Id.to_S();
                            }
                        }
                    }

                    //removing obsolete/malformed plans
                    if (plans.ContainsKey(false))
                    {
                        List<PlanDTO> obsoletePlans = plans[false];
                        Logger.LogInfo($"#### Found {obsoletePlans.Count} obsolete MADSE plans", DocuSignManager.DocusignTerminalName);
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
                PlanState = PlanState.Running,
                Visibility = PlanVisibility.Internal
            };

            var monitorDocusignPlan = await _hubCommunicator.CreatePlan(emptyMonitorPlan, curFr8UserId);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null, curFr8UserId);
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Prepare_DocuSign_Events_For_Storage");
            var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "SaveToFr8Warehouse");
            Debug.WriteLine($"Calling create and configure with params {recordDocusignEventsTemplate} {curFr8UserId} {monitorDocusignPlan}");
            await _hubCommunicator.CreateAndConfigureActivity(recordDocusignEventsTemplate.Id,
                curFr8UserId, "Record DocuSign Events", 1, monitorDocusignPlan.Plan.StartingSubPlanId, false, new Guid(authTokenDTO.Id));
            var storeMTDataActivity = await _hubCommunicator.CreateAndConfigureActivity(storeMTDataTemplate.Id,
                curFr8UserId, "Save To Fr8 Warehouse", 2, monitorDocusignPlan.Plan.StartingSubPlanId);
            SetSelectedCrates(storeMTDataActivity);
            //save this
            await _hubCommunicator.ConfigureActivity(storeMTDataActivity, curFr8UserId);
            await _hubCommunicator.RunPlan(monitorDocusignPlan.Plan.Id, new List<CrateDTO>(),  curFr8UserId);

            Logger.LogInfo($"#### New MADSE plan activated with planId: {monitorDocusignPlan.Plan.Id}", DocuSignManager.DocusignTerminalName);
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