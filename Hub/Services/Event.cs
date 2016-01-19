using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using System.Configuration;
using Data.Crates;
using Data.Interfaces.Manifests;
using Data.Interfaces;
using Data.States;
using Data.Entities;
using System.Linq;
using System.Collections.Generic;
using Data.Exceptions;
using Utilities;

namespace Hub.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {

        private readonly ITerminal _terminal;
        private readonly IRoute _route;

        public Event()
        {

            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _route = ObjectFactory.GetInstance<IRoute>();
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
        //    IList<RouteDO> matchingRoutes = _route.GetMatchingRoutes(userID, curEventReport);
        //    using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        foreach (var subroute in matchingRoutes)
        //        {
        //            //4. When there's a match, it means that it's time to launch a new Process based on this Route, 
        //            //so make the existing call to Route#LaunchProcess.
        //            _route.LaunchProcess(unitOfWork, subroute);
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
            if (eventReportMS.EventPayload == null)
            {
                throw new ArgumentException("EventReport can't have a null payload");
            }
            if (eventReportMS.ExternalAccountId == null)
            {
                throw new ArgumentException("EventReport can't have a null ExternalAccountId");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (eventReportMS.ExternalAccountId == systemUserEmail)
                {
                    Fr8AccountDO systemUser = uow.UserRepository.FindOne(u => u.Email == systemUserEmail);
                    await FindAccountRoutes(uow, eventReportMS, curCrateStandardEventReport, systemUser);
                }
                else
                {
                    //find the corresponding DockyardAccount
                    var authTokenList = uow.AuthorizationTokenRepository.FindList(x => x.ExternalAccountId == eventReportMS.ExternalAccountId);
                    if (authTokenList == null)
                    {
                        return;
                    }

                    foreach (var authToken in authTokenList)
                    {
                        var curDockyardAccount = authToken.UserDO;

                        await FindAccountRoutes(uow, eventReportMS, curCrateStandardEventReport, curDockyardAccount);
                    }
                }

            }
        }

        private async Task FindAccountRoutes(IUnitOfWork uow, EventReportCM eventReportMS,
               Crate curCrateStandardEventReport, Fr8AccountDO curDockyardAccount = null)
        {
            //find this Account's Routes
            var initialRoutesList = uow.RouteRepository
                .FindList(pt => pt.Fr8Account.Id == curDockyardAccount.Id)
                .Where(x => x.RouteState == RouteState.Active);

            var subscribingRoutes = _route.MatchEvents(initialRoutesList.ToList(), eventReportMS);

            await LaunchProcesses(subscribingRoutes, curCrateStandardEventReport);
        }

        public Task LaunchProcesses(List<RouteDO> curRoutes, Crate curEventReport)
        {
            var processes = new List<Task>();

            foreach (var curRoute in curRoutes)
            {
                //4. When there's a match, it means that it's time to launch a new Process based on this Route, 
                //so make the existing call to Route#LaunchProcess.
                processes.Add(LaunchProcess(curRoute, curEventReport));
            }

            return Task.WhenAll(processes);
        }

        public async Task LaunchProcess(RouteDO curRoute, Crate curEventData)
        {
            if (curRoute == null)
                throw new EntityNotFoundException(curRoute);

            if (curRoute.RouteState != RouteState.Inactive)
            {
                await _route.Run(curRoute, curEventData);
            }
        }
    }
}

