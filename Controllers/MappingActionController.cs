using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Web.Controllers
{
	[RoutePrefix("api/mapping_actions")]
	public class MappingActionController : ApiController
	{
		private readonly IAction _action;

		public MappingActionController()
		{
			_action = ObjectFactory.GetInstance<IAction>();
		}
		[Route("upstream")]
		[ResponseType(typeof(IEnumerable<ActionDTOBase>))]
		public IHttpActionResult GetUpstreamActivities(ActionDesignDTO actionDesignDTO)
		{
			ActionDO actionDO = Mapper.Map<ActionDO>(actionDesignDTO);
			var upstreamActivities = _action.GetUpstreamActivities(actionDO);
			var result = upstreamActivities.Select(x => Mapper.Map<ActionDTOBase>(x)).ToList();
			return Ok(result);
		}
	}
}