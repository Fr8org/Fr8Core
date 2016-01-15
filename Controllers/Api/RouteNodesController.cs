using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;

namespace HubWeb.Controllers
{
    //[RoutePrefix("route_nodes")]
    [Fr8ApiAuthorize]
    public class RouteNodesController : ApiController
    {
        private readonly IRouteNode _activity;
        private readonly ISecurityServices _security;

        public RouteNodesController()
        {
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        [HttpGet]
        [ResponseType(typeof (ActivityTemplateDTO))]
        public IHttpActionResult Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActivityTemplateDO = uow.ActivityTemplateRepository.GetByKey(id);

                var curActivityTemplateDTO =
                    Mapper.Map<ActivityTemplateDTO>(curActivityTemplateDO);

                return Ok(curActivityTemplateDTO);
            }
        }

        [ActionName("upstream")]
        [ResponseType(typeof (List<RouteNodeDO>))]
        public IHttpActionResult GetUpstreamActivities(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionDO = uow.ActionRepository.GetByKey(id);
                var upstreamActivities = _activity.GetUpstreamActivities(uow, actionDO);
                return Ok(upstreamActivities);
            }
        }

        [ActionName("downstream")]
        [ResponseType(typeof (List<RouteNodeDO>))]
        public IHttpActionResult GetDownstreamActivities(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionDO actionDO = uow.ActionRepository.GetByKey(id);
                var downstreamActivities = _activity.GetDownstreamActivities(uow, actionDO);
                return Ok(downstreamActivities);
            }
        }

        // TODO: after DO-1214 is completed, this method must be removed.
        [ActionName("upstream_actions")]
        [ResponseType(typeof (List<ActionDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetUpstreamActions(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionDO = uow.ActionRepository.GetByKey(id);
                var upstreamActions = _activity
                    .GetUpstreamActivities(uow, actionDO)
                    .OfType<ActionDO>()
                    .Select(x => Mapper.Map<ActionDTO>(x))
                    .ToList();

                return Ok(upstreamActions);
            }
        }

        // TODO: after DO-1214 is completed, this method must be removed.
        [ActionName("downstream_actions")]
        [ResponseType(typeof (List<ActionDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetDownstreamActions(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionDO actionDO = uow.ActionRepository.GetByKey(id);
                var downstreamActions = _activity
                    .GetDownstreamActivities(uow, actionDO)
                    .OfType<ActionDO>()
                    .Select(x => Mapper.Map<ActionDTO>(x))
                    .ToList();

                return Ok(downstreamActions);
            }
        }

        [ActionName("downstream_fields")]
        [ResponseType(typeof(List<ActionDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetDownstreamFields(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionDO actionDO = uow.ActionRepository.GetByKey(id);
                var downstreamActions = _activity
                    .GetDownstreamActivities(uow, actionDO)
                    .OfType<ActionDO>()
                    .Select(x => x.CrateStorage)
                    .ToList();



            }
        }       

        [ActionName("available")]
        [ResponseType(typeof (IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetAvailableActivities()
        {
            var categoriesWithActivities = _activity.GetAvailableActivitiyGroups();

            return Ok(categoriesWithActivities);
        }

        [ActionName("available")]
        [ResponseType(typeof (IEnumerable<ActivityTemplateDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetAvailableActivities(string tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Func<ActivityTemplateDO, bool> predicate = (at) =>
                    string.IsNullOrEmpty(at.Tags) ? false :
                        at.Tags.Split(new char[] {','}).Any(c => string.Equals(c.Trim(), tag, StringComparison.InvariantCultureIgnoreCase));
                var categoriesWithActivities = _activity.GetAvailableActivities(uow, tag == "[all]" ? (at) => true : predicate);
                return Ok(categoriesWithActivities);
            }
        }
    }
}