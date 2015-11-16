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
    [RoutePrefix("route_nodes")]
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

        [Route("upstream")]
        [ResponseType(typeof (List<RouteNodeDO>))]
        public IHttpActionResult GetUpstreamActivities(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionDO = uow.ActionRepository.GetByKey(id);
                var upstreamActivities = _activity.GetUpstreamActivities(uow, actionDO);
                return Ok(upstreamActivities);
            }
        }

        [Route("downstream")]
        [ResponseType(typeof (List<RouteNodeDO>))]
        public IHttpActionResult GetDownstreamActivities(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionDO actionDO = uow.ActionRepository.GetByKey(id);
                var downstreamActivities = _activity.GetDownstreamActivities(uow, actionDO);
                return Ok(downstreamActivities);
            }
        }

        // TODO: after DO-1214 is completed, this method must be removed.
        [Route("upstream_actions")]
        [ResponseType(typeof (List<ActionDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetUpstreamActions(int id)
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
        [Route("downstream_actions")]
        [ResponseType(typeof (List<ActionDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetDownstreamActions(int id)
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

        [Route("available")]
        [ResponseType(typeof (IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetAvailableActivities()
        {
            var categoriesWithActivities = _activity.GetAvailableActivitiyGroups();

            return Ok(categoriesWithActivities);
        }

        [Route("solution")]
        [ResponseType(typeof(IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetSolutions()
        {
            var categoriesWithActivities = _activity.GetSolutions();

            return Ok(categoriesWithActivities);
        }

        [Route("available")]
        [ResponseType(typeof (IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        public IHttpActionResult GetAvailableActivities(string tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Func<ActivityTemplateDO, bool> predicate = (at) =>
                    string.IsNullOrEmpty(at.Tags) ? false :
                        at.Tags.Split(new char[] {','}).Any(c => string.Equals(c.Trim(), tag, StringComparison.InvariantCultureIgnoreCase));
                var categoriesWithActivities = _activity.GetAvailableActivities(uow, predicate);
                return Ok(categoriesWithActivities);
            }
        }
    }
}