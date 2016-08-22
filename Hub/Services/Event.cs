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
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.Infrastructure.Data.States;

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
        private IFr8Account _fr8Account;

        public Event()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _plan = ObjectFactory.GetInstance<IPlan>();
            _fr8Account = ObjectFactory.GetInstance<IFr8Account>();
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
            var inboundEvent = curCrateStandardEventReport.Get<EventReportCM>();
            if (string.IsNullOrWhiteSpace(inboundEvent.ExternalDomainId) && string.IsNullOrWhiteSpace(inboundEvent.ExternalAccountId))
            {
                Logger.GetLogger().Error($"External event has no information about external account or external domain. Processing is cancelled. Event names - {inboundEvent.EventNames}, " +
                                $"manufacturer - {inboundEvent.Manufacturer} ");
                return;
            }

            var systemUser = _fr8Account.GetSystemUser();
            if (systemUser == null)
                throw new ApplicationException("System User Account is Missing");
            string systemUserEmail = systemUser.UserName;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Logger.GetLogger().Info($"Received external event for account '{inboundEvent.ExternalAccountId}'");
                if (inboundEvent.ExternalAccountId == systemUserEmail)
                {
                    try
                    {
                        var eventCm = curCrateStandardEventReport.Get<EventReportCM>();

                        EventRouter currentRouter = GetEventRouter(eventCm);

                        var errorMsgList = new List<string>();
                        foreach (var crate in eventCm.EventPayload)
                        {
                            if (crate.ManifestType.Id != (int)Fr8.Infrastructure.Data.Constants.MT.LoggingData)
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
                    //Find the corresponding Fr8 accounts
                    var authTokens = uow.AuthorizationTokenRepository.GetPublicDataQuery();
                    if (!string.IsNullOrWhiteSpace(inboundEvent.ExternalDomainId))
                    {
                        authTokens = authTokens.Where(x => x.ExternalDomainId == inboundEvent.ExternalDomainId);
                    }
                    //If external account Id doesn't exist it means that event is domain-wide i.e. it relates to all accounts that belong to specified domain
                    if (!string.IsNullOrWhiteSpace(inboundEvent.ExternalAccountId))
                    {
                        authTokens = authTokens.Where(x => x.ExternalAccountId == inboundEvent.ExternalAccountId);
                    }
                    //Checking both domain and account is additional way to protect from running plans not related to the event as account Id is often an email and can be the same across
                    //multiple terminals
                    var planOwnerIds = authTokens.Select(x => x.UserID).Distinct().ToArray();
                    Logger.GetLogger().Info($"External event for domain '{inboundEvent.ExternalDomainId}' and account '{inboundEvent.ExternalAccountId}' relates to {planOwnerIds.Length} user(s)");
                    if (string.IsNullOrEmpty(inboundEvent.ExternalDomainId) && planOwnerIds.Length > 1)
                    {
                        Logger.GetLogger().Warn($"Multiple users are identified as owners of plans related to external domain '{inboundEvent.ExternalDomainId}' and account '{inboundEvent.ExternalAccountId}'");
                    }
                    foreach (var planOwnerId in planOwnerIds)
                    {
                        try
                        {
                            FindAndExecuteAccountPlans(uow, inboundEvent, curCrateStandardEventReport, planOwnerId);
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
                .Where(pt => pt.Fr8AccountId == curDockyardAccountId && (pt.PlanState == PlanState.Executing || pt.PlanState == PlanState.Active)).ToList();
            var subscribingPlans = _plan.MatchEvents(initialPlansList, eventReportMS);

            Logger.GetLogger().Info($"Upon receiving event for account '{eventReportMS.ExternalAccountId}' {subscribingPlans.Count} of {initialPlansList.Count} will be notified");
            //When there's a match, it means that it's time to launch a new Process based on this Plan, 
            foreach (var plan in subscribingPlans.Where(p => p.PlanState != PlanState.Inactive))
            {
                _plan.Enqueue(plan.Id, curCrateStandardEventReport);
            }
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