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
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
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

        [ResponseType(typeof(List<ActivityDTO>))]
        [Fr8HubWebHMACAuthenticate]
        public IHttpActionResult Get(Guid id, string direction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityDO activityDO = uow.PlanRepository.GetById<ActivityDO>(id);
                switch(direction)
                {
                    case "upstream":
                        return Ok(_activity.GetUpstreamActivities(uow, activityDO).
                            OfType<ActivityDO>().Select(x => Mapper.Map<ActivityDTO>(x)).ToList());
                    case "downstream":
                        return Ok(_activity.GetDownstreamActivities(uow, activityDO).
                            OfType<ActivityDO>().Select(x => Mapper.Map<ActivityDTO>(x)).ToList());
                    default:
                        return Ok();
                }
            }
        }

        [HttpGet]
        [ActionName("signals")]
        [Fr8HubWebHMACAuthenticate]
        public IHttpActionResult GetAvailableData(Guid id, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)
        {
            return Ok(_activity.GetIncomingData(id, direction, availability));
        }
    }
}