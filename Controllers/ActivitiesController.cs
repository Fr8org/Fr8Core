using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Core.Managers;
using Data.Infrastructure.StructureMap;
using Microsoft.AspNet.Identity;

namespace Web.Controllers
{
    [RoutePrefix("activities")]
	public class ActivitiesController : ApiController
	{
      	private readonly IActivity _activity;
      	private readonly ISecurityServices _security;

		public ActivitiesController()
		{
			_activity = ObjectFactory.GetInstance<IActivity>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
		}

		[Route("upstream")]
		[ResponseType(typeof(List<ActivityDO>))]
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
		[ResponseType(typeof(List<ActivityDO>))]
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

        [fr8ApiAuthorize]
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