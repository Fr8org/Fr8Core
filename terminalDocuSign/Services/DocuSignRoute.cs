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

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related routes in Hub
    /// </summary>
    public class DocuSignRoute : IDocuSignRoute
    {
        /// <summary>
        /// Creates Monitor All DocuSign Events route with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        public async Task CreateRoute_MonitorAllDocuSignEvents(string curFr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curFr8Account = uow.UserRepository.GetByKey(curFr8UserId);

                //if route already created
                if (uow.RouteRepository.GetQuery().Any(existingRoute =>
                                                            existingRoute.Name.Equals("MonitorAllDocuSignEvents") &&
                                                            existingRoute.Fr8Account.Email.Equals(curFr8Account.Email)))
                {
                    return;
                }

                //Create a route
                RouteDO route = new RouteDO
                {
                    Name = "MonitorAllDocuSignEvents",
                    Description = "Monitor All DocuSign Events",
                    Fr8Account = curFr8Account,
                    RouteState = RouteState.Active,
                    Tag = "Monitor"
                };
                uow.RouteRepository.Add(route);
                uow.SaveChanges();

                //get activity templates of required actions
                var _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
                var activity1 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "Record_DocuSign_Events_v1"));
                var activity2 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "StoreMTData_v1"));

                //create and configure required actions
                var _action = ObjectFactory.GetInstance<IAction>();
                var action1 = await _action.CreateAndConfigure(uow, curFr8UserId, activity1.Id, activity1.Name, activity1.Label, route.Id);
                var action2 = await _action.CreateAndConfigure(uow, curFr8UserId, activity2.Id, activity2.Name, activity2.Label, route.Id);

                //add actions into route nodes table
                uow.RouteNodeRepository.Add(action1);
                uow.RouteNodeRepository.Add(action2);

                //create sub route with all requried actions
                var subroute = new SubrouteDO(true)
                {
                    ParentRouteNode = route,
                    ChildNodes = new List<RouteNodeDO> { action1, action2 }
                };

                //assing the sub route node to route
                route.ChildNodes = new List<RouteNodeDO> { subroute };

                //update them into the repositories
                uow.RouteNodeRepository.Add(subroute);
                uow.SaveChanges();
            }
        }
    }
}