using System;
using System.Threading.Tasks;
using StructureMap;
using Data.Infrastructure;
using Hub.Interfaces;
using Data.Interfaces;
using Data.States;
using Data.Entities;
using System.Linq;
using System.Collections.Generic;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Utilities.Logging;
using Fr8Data.Managers;

namespace Hub.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {
        private delegate void EventRouter(LoggingDataCM loggingDataCm);
        private readonly ITerminal _terminal;
        private readonly IPlan _plan;
        private ICrateManager _crateManager;

        public Event()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _plan = ObjectFactory.GetInstance<IPlan>();
        }
        /// <see cref="IEvent.HandleTerminalIncident"/>
        public void HandleTerminalIncident(LoggingDataCM incident)
        {
            EventManager.ReportTerminalIncident(incident);
        }

        public void HandleTerminalEvent(LoggingDataCM eventDataCm)
        {
            EventManager.ReportTerminalEvent(eventDataCm);
        }

        private EventRouter GetEventRouter(EventReportCM eventCm)
        {
            if (eventCm.EventNames.Equals("Terminal Incident"))
            {
                return HandleTerminalIncident;
            }

            if (eventCm.EventNames.Equals("Terminal Fact"))
            {
                return HandleTerminalEvent;
            }

            throw new InvalidOperationException("Unknown EventDTO with name: " + eventCm.EventNames);
        }

        public async Task ProcessInboundEvents(Crate curCrateStandardEventReport)
        {
            var eventReportMS = curCrateStandardEventReport.Get<EventReportCM>();

            // Fetching values from Config file is not working on CI.
            //var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            //string systemUserEmail = configRepository.Get("SystemUserEmail");

            string systemUserEmail = "system1@fr8.co";

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Logger.LogInfo($"Received external event for account '{eventReportMS.ExternalAccountId}'");
                if (eventReportMS.ExternalAccountId == systemUserEmail)
                {
                    try
                    {
                        var eventCm = curCrateStandardEventReport.Get<EventReportCM>();

                        EventRouter currentRouter = GetEventRouter(eventCm);

                        var errorMsgList = new List<string>();
                        foreach (var crate in eventCm.EventPayload)
                        {
                            if (crate.ManifestType.Id != (int)Fr8Data.Constants.MT.LoggingData)
                            {
                                errorMsgList.Add("Don't know how to process an EventReport with the Contents: " + _crateManager.ToDto(crate));
                                continue;
                            }

                            var loggingData = crate.Get<LoggingDataCM>();
                            currentRouter(loggingData);
                        }

                        if (errorMsgList.Count > 0)
                        {
                            throw new InvalidOperationException(String.Join(";;;", errorMsgList));
                        }
                    }
                    catch (Exception ex)
                    {
                        EventManager.UnexpectedError(ex);
                    }
                }
                else
                {
                    //find the corresponding DockyardAccount
                    //For team-wide events we use ExternalDomainId property (e.g. received Slack message should run all plans for respective Slack team)
                    var authTokenList = uow.AuthorizationTokenRepository
                        .GetPublicDataQuery()
                        .Where(x => x.ExternalAccountId == eventReportMS.ExternalAccountId
                                || (x.ExternalDomainId != null && x.ExternalDomainId == eventReportMS.ExternalDomainId))
                        .ToArray();
                    var planOwnerIds = authTokenList.Select(x => x.UserID).Distinct().ToArray();
                    Logger.LogInfo($"External event for account '{eventReportMS.ExternalAccountId}' relates to {authTokenList.Length} auth tokens of {planOwnerIds.Length} user(s)");
                    if (string.IsNullOrEmpty(eventReportMS.ExternalDomainId) && planOwnerIds.Length > 1)
                    {
                        Logger.LogWarning($"Multiple users are identified as owners of plans related to external account '{eventReportMS.ExternalAccountId}'");
                    }
                    foreach (var planOwnerId in planOwnerIds)
                    {
                        try
                        {
                            FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, planOwnerId);
                        }
                        catch (Exception ex)
                        {
                            EventManager.UnexpectedError(ex);
                        }
                    }
                }
            }
        }

        private void FindAndExecuteAccountPlans(
            IUnitOfWork uow, 
            EventReportCM eventReportMS,
            Crate curCrateStandardEventReport, 
            string curDockyardAccountId = null)
        {
            //find this Account's Plans
            var initialPlansList = uow.PlanRepository.GetPlanQueryUncached()
                .Where(pt => pt.Fr8AccountId == curDockyardAccountId && pt.PlanState == PlanState.Running).ToList();
            var subscribingPlans = _plan.MatchEvents(initialPlansList, eventReportMS);

            Logger.LogInfo($"Upon receiving event for account '{eventReportMS.ExternalAccountId}' {subscribingPlans.Count} of {initialPlansList.Count} will be notified");
            //When there's a match, it means that it's time to launch a new Process based on this Plan, 
            //so make the existing call to Plan#LaunchProcess.
            _plan.Enqueue(subscribingPlans.Where(p => p.PlanState != PlanState.Inactive).ToList(),  curCrateStandardEventReport);
        }

        public Task LaunchProcess(PlanDO curPlan, Crate curEventData = null)
        {
            throw new NotImplementedException();
        }

        Task IEvent.LaunchProcesses(List<PlanDO> curPlans, Crate curEventReport)
        {
            throw new NotImplementedException();
        }
    }
}