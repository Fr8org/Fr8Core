using System;
using System.Web.Http;
using AutoMapper;
using Data.Interfaces;
using StructureMap;
using System.Web.Http.Description;
using System.Collections.Generic;
using Hub.Interfaces;
using Data.Entities;
using System.Linq;
using System.Net;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class ActivityTemplatesController : ApiController
    {
        private readonly IPlanNode _activity;
        
        public ActivityTemplatesController()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
        }
        /// <summary>
        /// Retreives activity template with specified Id
        /// </summary>
        /// <param name="id">Id of activity template to retrieve</param>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "Retrieved activity template", typeof(ActivityTemplateDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Activity template doesn't exist", typeof(DetailedMessageDTO))]
        public IHttpActionResult Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository.GetByKey(id);
                return Ok(Mapper.Map<ActivityTemplateDTO>(activityTemplate));
            }
        }


        /// <summary>
        /// Retreives all available activity templates grouped by category
        /// </summary>
        /// <response code="200">Collection of activity templates grouped by category</response>
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<ActivityTemplateCategoryDTO>))]
        public IHttpActionResult Get()
        {
            var categoriesWithActivities = _activity.GetActivityTemplatesGroupedByCategories();
            return Ok(categoriesWithActivities);
        }

        /// <summary>
        /// Retreives all activity templates that are tagged with specified value
        /// </summary>
        /// <param name="tag">Value of tag to filter activities by</param>
        /// <response code="200">Collection of activity templates</response>  
        [ResponseType(typeof(IEnumerable<ActivityTemplateDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult Get(string tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Func<ActivityTemplateDO, bool> predicate = at =>
                    !string.IsNullOrEmpty(at.Tags) && at.Tags.Split(',').Any(c => string.Equals(c.Trim(), tag, StringComparison.InvariantCultureIgnoreCase));
                var categoriesWithActivities = _activity.GetAvailableActivities(uow, tag == "[all]" ? at => true : predicate);
                return Ok(categoriesWithActivities);
            }
        }
    }
}