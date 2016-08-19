using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using log4net;
using StructureMap;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related plans in Hub
    /// </summary>
    public class DocuSignPlan : IDocuSignPlan
    {
        private static readonly ILog Logger = LogManager.GetLogger(DocuSignManager.DocusignTerminalName);

        private readonly ICrateManager _crateManager;
        private readonly IDocuSignManager _docuSignManager;
        private readonly IDocuSignConnect _docuSignConnect;
        private readonly IHubEventReporter _eventReporter;

        private readonly string DevConnectName = "(dev) Fr8 Company DocuSign integration";
        private readonly string DemoConnectName = "(demo) Fr8 Company DocuSign integration";
        private readonly string ProdConnectName = "Fr8 Company DocuSign integration";
        private readonly string TemporaryConnectName = "int-tests-Fr8";


        public DocuSignPlan(ICrateManager crateManager, IDocuSignManager docuSignManager, IDocuSignConnect docuSignConnect, IHubEventReporter eventReporter)
        {
            _crateManager = crateManager;
            _docuSignManager = docuSignManager;
            _docuSignConnect = docuSignConnect;
            _eventReporter = eventReporter;
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// 
        /// https://maginot.atlassian.net/wiki/display/DDW/Rework+of+DocuSign+connect+management
        /// </summary>
        public async Task CreatePlan_MonitorAllDocuSignEvents(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            Logger.Info($"Create MADSE called {authToken.UserId}");
            await RemoveInactiveExistingPlans(hubCommunicator, "MonitorAllDocuSignEvents", authToken);

            await CreateAndActivateNewMADSEPlan(hubCommunicator, authToken);
        }

        //only create a connect when running on dev/production
        public void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            Logger.Info($"CreateConnect called {authToken.UserId}");
            var authTokenDO = new AuthorizationTokenDO() { Token = authToken.Token, ExternalAccountId = authToken.ExternalAccountId };
            var config = _docuSignManager.SetUp(authToken);
            string terminalUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint");
            string prodUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultProductionUrl");
            string devUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDevUrl");
            string demoUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.DefaultDemoUrl");
            bool isSelfHosting = CloudConfigurationManager.GetSetting("terminalDocusign.NotSelfHosting") == null;
            string connectName = "";
            string connectId = "";

            Logger.Info($"CreateConnect terminalUrl {terminalUrl}");
            if (!isSelfHosting)
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
                    Logger.Info($"Unable to set connectName from {terminalUrl}");
                }

                string publishUrl = terminalUrl + "/terminals/terminalDocuSign/events";

                Logger.Info("Connect creation: publishUrl = {0}");

                if (!string.IsNullOrEmpty(connectName))
                {
                    connectId = _docuSignConnect.CreateOrActivateConnect(config, connectName, publishUrl);
                    Logger.Info($"Created connect named {connectName} pointing to {publishUrl} with id {connectId}");
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
                    Logger.Info($"Created connect named {TemporaryConnectName} pointing to {publishUrl} with id {connectId}");
                }
            }
            else
            {
                Logger.Info($"terminalUrl is empty, no work has been done in DocuSignPlan.CreateConnect: prodUrl -> {prodUrl}, devUrl -> {devUrl}, demoUrl -> {demoUrl}");
            }
        }

        public void CreateOrUpdatePolling(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            DocuSignPolling polling = new DocuSignPolling(_docuSignManager, _eventReporter);
            polling.SchedulePolling(hubCommunicator, authToken.ExternalAccountId);
        }


        private bool CheckIfSaveToFr8WarehouseConfiguredWithOldManifest(PlanDTO val)
        {
            return (val.SubPlans.ElementAt(0).Activities[1].ActivityTemplate.Name == "Save_All_Payload_To_Fr8_Warehouse");
        }

        private async Task RemoveInactiveExistingPlans(IHubCommunicator hubCommunicator, string plan_name, AuthorizationToken authToken)
        {
            try
            {
                var existingPlans = (await hubCommunicator.GetPlansByName(plan_name, PlanVisibility.Internal)).ToList();
                if (existingPlans.Count > 0)
                {
                    //active MADSE plans are likely related to another DS account
                    var plansForRemoval = existingPlans
                        .Where(val =>
                    (val.PlanState == PlanState.IntToString(PlanState.Inactive)) || //or all from above
                    !(val.SubPlans.Any() && val.SubPlans.ElementAt(0).Activities.Any() &&
                    val.SubPlans.ElementAt(0).Activities[1].ActivityTemplate.Name == "Save_All_Payload_To_Fr8_Warehouse")).ToList();

                    foreach (var plan in plansForRemoval)
                    {
                        await hubCommunicator.DeletePlan(plan.Id);
                    }

                    Logger.Info($"Removed {plansForRemoval.Count} obsolete MADSE plan");
                }
            }
            catch (Exception exc) { Logger.Info("Failed to remove obsolete MADSE plan"); }
        }

        private async Task CreateAndActivateNewMADSEPlan(IHubCommunicator hubCommunicator, AuthorizationToken authToken)
        {
            try
            {
                var emptyMonitorPlan = new PlanNoChildrenDTO
                {
                    Name = "MonitorAllDocuSignEvents",
                    Description = "MonitorAllDocuSignEvents",
                    PlanState = "Active",
                    Visibility = new PlanVisibilityDTO() { Hidden = true }
                };

                var monitorDocusignPlan = await hubCommunicator.CreatePlan(emptyMonitorPlan);
                Debug.WriteLine("Attemting to create a new MADSE plan");
                var activityTemplates = await hubCommunicator.GetActivityTemplates();
                var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Prepare_DocuSign_Events_For_Storage");
                var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "Save_All_Payload_To_Fr8_Warehouse");
                Debug.WriteLine($"Calling create and configure with params {recordDocusignEventsTemplate} {authToken.UserId} {monitorDocusignPlan}");
                await hubCommunicator.CreateAndConfigureActivity(recordDocusignEventsTemplate.Id, "Record DocuSign Events", 1, monitorDocusignPlan.StartingSubPlanId, false, new Guid(authToken.Id));
                var storeMTDataActivity = await hubCommunicator.CreateAndConfigureActivity(storeMTDataTemplate.Id, "Save To Fr8 Warehouse", 2, monitorDocusignPlan.StartingSubPlanId);

                //save this
                await hubCommunicator.ConfigureActivity(storeMTDataActivity);
                await hubCommunicator.RunPlan(monitorDocusignPlan.Id, null);
                Logger.Info($"#### New MADSE plan activated with planId: {monitorDocusignPlan.Id}");
            }
            catch (Exception exc)
            {
                Logger.Error($"#### Failed to create MADSE plan for {authToken.ExternalAccountId} Exception:{exc.Message} {exc.StackTrace.Substring(255)}");
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