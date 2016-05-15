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
using System.Data.Entity;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Utilities.Logging;

namespace Hub.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {

        private readonly ITerminal _terminal;
        private readonly IPlan _plan;

        public Event()
        {
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _plan = ObjectFactory.GetInstance<IPlan>();
        }
        /// <see cref="IEvent.HandleTerminalIncident"/>
        public void HandleTerminalIncident(LoggingDataCm incident)
        {
            EventManager.ReportTerminalIncident(incident);
        }

        public void HandleTerminalEvent(LoggingDataCm eventDataCm)
        {
            EventManager.ReportTerminalEvent(eventDataCm);
        }

        //public void ProcessInbound(string userID, EventReportMS curEventReport)
        //{
        //    //check if CrateDTO is not null
        //    if (curEventReport == null)
        //        throw new ArgumentNullException("Paramter Standard Event Report is null.");

        //    //Matchup process
        //    IList<PlanDO> matchingPlans = _plan.GetMatchingPlans(userID, curEventReport);
        //    using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        foreach (var subPlan in matchingPlans)
        //        {
        //            //4. When there's a match, it means that it's time to launch a new Process based on this Plan, 
        //            //so make the existing call to Plan#LaunchProcess.
        //            _plan.LaunchProcess(unitOfWork, subPlan);
        //        }
        //    }
        //}

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
                        Fr8AccountDO systemUser = uow.UserRepository.GetOrCreateUser(systemUserEmail);
                        FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, systemUser.Id);
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
                        .Where(x => x.ExternalAccountId.Contains(eventReportMS.ExternalAccountId)
                                || (x.ExternalDomainId != null && x.ExternalDomainId == eventReportMS.ExternalDomainId))
                        .ToArray();
                    Logger.LogInfo($"External event for account '{eventReportMS.ExternalAccountId}' relates to {authTokenList.Length} auth tokens");
                    foreach (var authToken in authTokenList)
                    {
                        try
                        {
                            FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, authToken.UserID);
                        }
                        catch (Exception ex)
                        {
                            EventManager.UnexpectedError(ex);
                        }
                    }
                }
            }
        }

        private void FindAndExecuteAccountPlans(IUnitOfWork uow, EventReportCM eventReportMS,
               Crate curCrateStandardEventReport, string curDockyardAccountId = null)
        {
            //find this Account's Plans
            var initialPlansList = uow.PlanRepository.GetPlanQueryUncached()
                .Where(pt => pt.Fr8AccountId == curDockyardAccountId && pt.PlanState == PlanState.Active).ToList();
            var subscribingPlans = _plan.MatchEvents(initialPlansList, eventReportMS);

            Logger.LogInfo($"Upon receiving event for account '{eventReportMS.ExternalAccountId}' {subscribingPlans.Count} of {initialPlansList.Count} will be notified");
            //When there's a match, it means that it's time to launch a new Process based on this Plan, 
            //so make the existing call to Plan#LaunchProcess.
            _plan.Enqueue(
                subscribingPlans.Where(p => p.PlanState != PlanState.Inactive).ToList(), 
                curCrateStandardEventReport
            );
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