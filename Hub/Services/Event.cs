using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Exceptions;
using Hub.Interfaces;
using System.Configuration;
using Data.Crates;
using Data.Interfaces.Manifests;
using Data.Interfaces;
using Data.States;
using Data.Entities;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using Data.Exceptions;
using Utilities;
using Hub.Managers;
using Hangfire;

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
                if (eventReportMS.ExternalAccountId == systemUserEmail)
                {
                    try
                    {
                        Fr8AccountDO systemUser = uow.UserRepository.GetOrCreateUser(systemUserEmail);
                        FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, systemUser);
                    }
                    catch (Exception ex)
                    {
                        EventManager.UnexpectedError(ex);
                    }
                }
                else
                {
                    //find the corresponding DockyardAccount
                    var authTokenList = uow.AuthorizationTokenRepository.GetPublicDataQuery()
                        .Include(x => x.UserDO).Where(x => x.ExternalAccountId.Contains(eventReportMS.ExternalAccountId)).ToArray();
                    var tasks = new List<Task>();
                    foreach (var authToken in authTokenList)
                    {
                        var curDockyardAccount = authToken.UserDO;
                        try
                        {
                            FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, curDockyardAccount);
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
               Crate curCrateStandardEventReport, Fr8AccountDO curDockyardAccount = null)
        {
            //find this Account's Plans
            var initialPlansList = uow.PlanRepository.GetPlanQueryUncached()
                .Where(pt => pt.Fr8AccountId == curDockyardAccount.Id && pt.PlanState == PlanState.Active).ToList();
            var subscribingPlans = _plan.MatchEvents(initialPlansList, eventReportMS);

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