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
	public class RouteNodesController : ApiController
	{
      	private readonly IRouteNode _activity;
      	private readonly ISecurityServices _security;

		public RouteNodesController()
		{
			_activity = ObjectFactory.GetInstance<IRouteNode>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
		}

		[Route("upstream")]
		[ResponseType(typeof(List<RouteNodeDO>))]
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
		[ResponseType(typeof(List<RouteNodeDO>))]
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
		[ResponseType(typeof(List<ActionDTO>))]
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
		[ResponseType(typeof(List<ActionDTO>))]
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

        [Fr8ApiAuthorize]
        [Route("available")]
        [ResponseType(typeof(IEnumerable<ActivityTemplateCategoryDTO>))]
        public IHttpActionResult GetAvailableActivities()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = _security.GetCurrentAccount(uow);
                var categoriesWithActivities = _activity.GetAvailableActivitiyGroups(curDockyardAccount);
                return Ok(categoriesWithActivities);
            }
        }
    }
}