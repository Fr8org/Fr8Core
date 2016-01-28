using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Configuration.Azure;

namespace Hub.Managers
{
    public class RouteManager
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;

        public RouteManager()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
        }

        public async Task CreateRoute_LogFr8InternalEvents(string curFr8UserId)
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curFr8Account = uow.UserRepository.GetOrCreateUser(curFr8UserId);

                //check if the plan already created

                var existingRoute = GetExistingRoute(uow, "LogFr8InternalEvents", curFr8Account.Email);

                if (existingRoute != null)
                {
                    //if plan is already created, just make it active and return
                    existingRoute.RouteState = RouteState.Active;
                    uow.SaveChanges();
                    return;
                }

                //Create a plan
                PlanDO plan = new PlanDO
                {
                    Name = "LogFr8InternalEvents",
                    Description = "Log Fr8Internal Events",
                    Fr8Account = curFr8Account,
                    RouteState = RouteState.Active,
                    Tag = "Monitor",
                    Id = Guid.NewGuid(),
                   
                };

                //create a sub plan
                var subroute = new SubrouteDO(true)
                {
                    ParentRouteNode = plan,
                    Id = Guid.NewGuid(),
                    RootRouteNode = plan
                };

                //update Route and Sub plan into database
                plan.ChildNodes = new List<RouteNodeDO> { subroute };
                uow.RouteNodeRepository.Add(plan);
                uow.RouteNodeRepository.Add(subroute);
                uow.SaveChanges();

                //get activity templates of required actions
                var activity1 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "Monitor_Fr8_Events_v1"));
                var activity2 = Mapper.Map<ActivityTemplateDTO>(_activityTemplate.GetByName(uow, "StoreMTData_v1"));

                //create and configure required actions
                await _activity.CreateAndConfigure(uow, curFr8Account.Id, activity1.Id, activity1.Name, activity1.Label, null, subroute.Id);
                await _activity.CreateAndConfigure(uow, curFr8Account.Id, activity2.Id, activity2.Name, activity2.Label, null, subroute.Id);

                //update database
                uow.SaveChanges();
            }
        }

        private PlanDO GetExistingRoute(IUnitOfWork uow, string routeName, string fr8AccountEmail)
        {
            if (uow.PlanRepository.GetQuery().Any(existingRoute =>
                existingRoute.Name.Equals(routeName) &&
                existingRoute.Fr8Account.Email.Equals(fr8AccountEmail)))
            {
                return uow.PlanRepository.GetQuery().First(existingRoute =>
                    existingRoute.Name.Equals(routeName) &&
                    existingRoute.Fr8Account.Email.Equals(fr8AccountEmail));
            }

            return null;
        }
    }
}