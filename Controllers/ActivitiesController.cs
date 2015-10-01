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
using Microsoft.AspNet.Identity;

namespace Web.Controllers
{
    [RoutePrefix("activities")]
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

        [DockyardAuthorize]
        [Route("available")]
        [ResponseType(typeof(IEnumerable<IEnumerable<ActivityTemplateDTO>>))]
        public IHttpActionResult GetAvailableActivities()
        {
            var userId = User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curDockyardAccount = uow.UserRepository.GetByKey(userId);

                var activityTemplateCategories = new List<List<ActivityTemplateDTO>>();
                foreach (var activity in _activity.GetAvailableActivities(curDockyardAccount))
                {
                    var activityTemplateDTO = Mapper.Map<ActivityTemplateDTO>(activity);

                    var properCategory = activityTemplateCategories.FirstOrDefault(cat => cat[0].Category == activityTemplateDTO.Category);
                    if (properCategory == null)
                    {
                        activityTemplateCategories.Add(new List<ActivityTemplateDTO> { Mapper.Map<ActivityTemplateDTO>(activityTemplateDTO) });
                    }
                    else
                    {
                        properCategory.Add( Mapper.Map<ActivityTemplateDTO>(activityTemplateDTO) );
                    }
                }
                return Ok(activityTemplateCategories);
            }
        }
	}
}