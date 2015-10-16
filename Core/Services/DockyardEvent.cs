using InternalInterfaces = Core.Interfaces;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;

namespace Core.Services
{
    public class DockyardEvent : IDockyardEvent
    {
        private readonly IRoute _route;
        private readonly InternalInterfaces.IContainer _process;
        private readonly ICrateManager _crate;

        public DockyardEvent()
        {
            _route = ObjectFactory.GetInstance<IRoute>();
            _process = ObjectFactory.GetInstance<InternalInterfaces.IContainer>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
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

        public async Task ProcessInboundEvents(CrateDTO curCrateStandardEventReport)
        {
            EventReportCM eventReportMS = _crate.GetContents<EventReportCM>(curCrateStandardEventReport);


            if (eventReportMS.EventPayload == null)
                throw new ArgumentException("EventReport can't have a null payload");
            if (eventReportMS.ExternalAccountId == null)
                throw new ArgumentException("EventReport can't have a null ExternalAccountId");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //find the corresponding DockyardAccount
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(at => at.ExternalAccountId == eventReportMS.ExternalAccountId);
                if (authToken == null)
                {
                    return;
                }

                var curDockyardAccount = authToken.UserDO;

                //find this Account's Routes
                var initialRoutesList = uow.RouteRepository
                    .FindList(pt => pt.Fr8Account.Id == curDockyardAccount.Id)
                    .Where(x => x.RouteState == RouteState.Active);

                var subscribingRoutes = _route.MatchEvents(initialRoutesList.ToList(),
                    eventReportMS);



                await LaunchProcesses(subscribingRoutes, curCrateStandardEventReport);
            
            }
        }

        public Task LaunchProcesses(List<RouteDO> curRoutes, CrateDTO curEventReport)
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

        public async Task LaunchProcess(RouteDO curRoute, CrateDTO curEventData)
        {
            if (curRoute == null)
                throw new EntityNotFoundException(curRoute);

            if (curRoute.RouteState != RouteState.Inactive)
            {
                await _process.Launch(curRoute, curEventData);
            }
        }
    }
}
