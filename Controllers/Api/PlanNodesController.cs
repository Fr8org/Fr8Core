using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.States;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    //[RoutePrefix("route_nodes")]
    public class PlanNodesController : ApiController
    {
        private readonly IPlanNode _activity;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IActivityTemplate _activityTemplate;

        public PlanNodesController()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        [HttpGet]
        [ResponseType(typeof(ActivityTemplateDTO))]
        [Fr8ApiAuthorize]
        public IHttpActionResult Get(Guid id)
        {
            var curActivityTemplateDO = _activityTemplate.GetByKey(id);
            var curActivityTemplateDTO = Mapper.Map<ActivityTemplateDTO>(curActivityTemplateDO);

            return Ok(curActivityTemplateDTO);
        }
       

        [ActionName("upstream")]
        [ResponseType(typeof(List<ActivityDTO>))]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult GetUpstreamActivities(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityDO = uow.PlanRepository.GetById<ActivityDO>(id);
                var upstreamActions = _activity
                    .GetUpstreamActivities(uow, activityDO)
                    .OfType<ActivityDO>()
                    .Select(x => Mapper.Map<ActivityDTO>(x))
                    .ToList();

                return Ok(upstreamActions);
            }
        }
        [ActionName("downstream")]
        [ResponseType(typeof(List<ActivityDTO>))]
        [Fr8HubWebHMACAuthenticate]
        public IHttpActionResult GetDownstreamActivities(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityDO activityDO = uow.PlanRepository.GetById<ActivityDO>(id);
                var downstreamActions = _activity
                    .GetDownstreamActivities(uow, activityDO)
                    .OfType<ActivityDO>()
                    .Select(x => Mapper.Map<ActivityDTO>(x))
                    .ToList();

                return Ok(downstreamActions);
            }
        }

        [HttpGet]
        [ActionName("available_data")]
        [Fr8HubWebHMACAuthenticate]
        public IHttpActionResult GetAvailableData(Guid id, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)
        {
            return Ok(_activity.GetIncomingData(id, direction, availability));
        }

        [ActionName("available")]
        [ResponseType(typeof(IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetAvailableActivities()
        {
            var categoriesWithActivities = _activity.GetAvailableActivityGroups();

            return Ok(categoriesWithActivities);
        }

        [ActionName("getAvailableActivitiesWithTag")]
        [ResponseType(typeof(IEnumerable<ActivityTemplateDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult getAvailableActivitiesWithTag(string tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Func<ActivityTemplateDO, bool> predicate = (at) =>
                    string.IsNullOrEmpty(at.Tags) ? false :
                        at.Tags.Split(new char[] { ',' }).Any(c => string.Equals(c.Trim(), tag, StringComparison.InvariantCultureIgnoreCase));
                var categoriesWithActivities = _activity.GetAvailableActivities(uow, tag == "[all]" ? (at) => true : predicate);
                return Ok(categoriesWithActivities);
            }
        }
    }
}