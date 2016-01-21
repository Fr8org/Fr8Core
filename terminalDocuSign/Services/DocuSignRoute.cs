using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using terminalDocuSign.Interfaces;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related routes in Hub
    /// </summary>
    public class DocuSignRoute : IDocuSignRoute
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IAction _action;
        private readonly IHubCommunicator _hubCommunicator;

        public DocuSignRoute()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _action = ObjectFactory.GetInstance<IAction>();
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events route with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        public async Task CreateRoute_MonitorAllDocuSignEvents(string curFr8UserId)
        {
            var newRoute = new RouteEmptyDTO
            {
                Name = "MonitorAllDocuSignEvents",
                Description = "MonitorAllDocuSignEvents",
                RouteState = RouteState.Active,
                Tag = "monitor"
            };
            newRoute = await _hubCommunicator.CreateRoute(newRoute, curFr8UserId);



            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curFr8Account = uow.UserRepository.GetByKey(curFr8UserId);

                //check if the route already created
                var existingRoute = GetExistingRoute(uow, "MonitorAllDocuSignEvents", curFr8Account.Email);
                if (existingRoute != null)
                {
                    //if route is already created, just make it active and return
                    existingRoute.RouteState = RouteState.Active;
                    uow.SaveChanges();
                    return;
                }

                //Create a route
                RouteDO route = new RouteDO
                {
                    Name = "MonitorAllDocuSignEvents",
                    Description = "Monitor All DocuSign Events",
                    Fr8Account = curFr8Account,
                    RouteState = RouteState.Active,
                    Tag = "Monitor",
                    Id = Guid.NewGuid()
                };

                //create a subroute
                var subroute = new SubrouteDO(true)
                {
                    Id = Guid.NewGuid(),
                    RootRouteNode = route,
                    ParentRouteNode = route
                };

                //update Route and Subroute into database
                route.ChildNodes = new List<RouteNodeDO> { subroute };
                uow.RouteNodeRepository.Add(route);
                uow.RouteNodeRepository.Add(subroute);
                uow.SaveChanges();

                //get activity templates of required actions
                var activity1 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "Record_DocuSign_Events_v1"));
                var activity2 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "StoreMTData_v1"));

                //create and configure required actions
                await _action.CreateAndConfigure(uow, curFr8UserId, activity1.Id, activity1.Name, activity1.Label, subroute.Id);
                await _action.CreateAndConfigure(uow, curFr8UserId, activity2.Id, activity2.Name, activity2.Label, subroute.Id);
                
                //update database
                uow.SaveChanges();
            }
        }

        private RouteDO GetExistingRoute(IUnitOfWork uow, string routeName, string fr8AccountEmail)
        {
            if (uow.RouteRepository.GetQuery().Any(existingRoute =>
                existingRoute.Name.Equals(routeName) &&
                existingRoute.Fr8Account.Email.Equals(fr8AccountEmail)))
            {
                return uow.RouteRepository.GetQuery().First(existingRoute =>
                    existingRoute.Name.Equals(routeName) &&
                    existingRoute.Fr8Account.Email.Equals(fr8AccountEmail));
            }

            return null;
        }
    }
}