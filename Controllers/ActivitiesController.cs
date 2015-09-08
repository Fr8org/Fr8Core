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

namespace Web.Controllers
{
	[RoutePrefix("mapping_actions")]
	public class ActivitiesController : ApiController
	{
		private readonly IActivity _activity;

		public ActivitiesController()
		{
			_activity = ObjectFactory.GetInstance<IActivity>();
		}
		[Route("upstream")]
		[ResponseType(typeof(List<ActivityDO>))]
		public IHttpActionResult GetUpstreamActivities(int id)
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				ActionDO actionDO = uow.ActionRepository.GetByKey(id);
				var upstreamActivities = _activity.GetUpstreamActivities(actionDO);
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
				var downstreamActivities = _activity.GetDownstreamActivities(actionDO);
				return Ok(downstreamActivities);
			}
		}
	}
}