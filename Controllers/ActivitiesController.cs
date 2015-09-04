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
		private readonly IAction _action;

		public ActivitiesController()
		{
			_action = ObjectFactory.GetInstance<IAction>();
		}
		[Route("upstream")]
		[ResponseType(typeof(IEnumerable<ActionDTOBase>))]
		public IHttpActionResult GetUpstreamActivities(int? id)
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				ActionDO actionDO = uow.ActionRepository.GetByKey(id);
				var upstreamActivities = _action.GetUpstreamActivities(actionDO);
				var result = upstreamActivities.Select(x => Mapper.Map<ActionDTOBase>(x)).ToList();
				return Ok(result);
			}
		}
		[Route("downstream")]
		[ResponseType(typeof(IEnumerable<ActionDTOBase>))]
		public IHttpActionResult GetDownstreamActivities(int? id)
		{
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				ActionDO actionDO = uow.ActionRepository.GetByKey(id);
				var downstreamActivities = _action.GetDownstreamActivities(actionDO);
				var result = downstreamActivities.Select(x => Mapper.Map<ActionDTOBase>(x)).ToList();
				return Ok(result);
			}
		}
	}
}