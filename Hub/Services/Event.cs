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
                        await FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, systemUser);
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
                        .Include(x => x.UserDO).Where(x => x.ExternalAccountId == eventReportMS.ExternalAccountId).ToArray();
                    var tasks = new List<Task>();
                    foreach (var authToken in authTokenList)
                    {
                        var curDockyardAccount = authToken.UserDO;
                        var accountTask = FindAndExecuteAccountPlans(uow, eventReportMS, curCrateStandardEventReport, curDockyardAccount);
                        tasks.Add(accountTask);
                    }
                    Task waitAllTask = null;
                    try
                    {
                        waitAllTask = Task.WhenAll(tasks.ToArray());
                        await waitAllTask;
                    }
                    catch
                    {
                        foreach (Exception ex in waitAllTask.Exception.InnerExceptions)
                        {
                            EventManager.UnexpectedError(ex);
                        }

                    }
                }

            }
        }

        private async Task FindAndExecuteAccountPlans(IUnitOfWork uow, EventReportCM eventReportMS,
               Crate curCrateStandardEventReport, Fr8AccountDO curDockyardAccount = null)
        {
            //find this Account's Plans
            var initialPlansList = uow.PlanRepository.GetPlanQueryUncached()
                .Where(pt => pt.Fr8AccountId == curDockyardAccount.Id && pt.PlanState == PlanState.Active).ToList();
            var subscribingPlans = _plan.MatchEvents(initialPlansList, eventReportMS);

            await LaunchProcesses(subscribingPlans, curCrateStandardEventReport);
        }

        public Task LaunchProcesses(List<PlanDO> curPlans, Crate curEventReport)
        {
            var processes = new List<Task>();

            foreach (var curPlan in curPlans)
            {
                //4. When there's a match, it means that it's time to launch a new Process based on this Plan, 
                //so make the existing call to Plan#LaunchProcess.
                processes.Add(LaunchProcess(curPlan, curEventReport));
            }

            return Task.WhenAll(processes);
        }

        public async Task LaunchProcess(PlanDO curPlan, Crate curEventData)
        {
            if (curPlan == null)
                throw new EntityNotFoundException(curPlan);

            if (curPlan.PlanState != PlanState.Inactive)
            {
                try
                {
                    await _plan.Run(curPlan, curEventData);
                }
                catch (Exception ex)
                {
                    EventManager.ContainerFailed(curPlan, ex);
                }
            }
        }
    }
}

