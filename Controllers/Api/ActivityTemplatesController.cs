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
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Controllers
{
    public class ActivityTemplatesController : ApiController
    {
        private readonly IPlanNode _activity;


        public ActivityTemplatesController()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
        }

        [HttpGet]
        public IHttpActionResult Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository.GetByKey(id);
                return Ok(Mapper.Map<ActivityTemplateDTO>(activityTemplate));
            }
        }

        [ResponseType(typeof(IEnumerable<ActivityTemplateCategoryDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult Get()
        {
            var categoriesWithActivities = _activity.GetAvailableActivityGroups();

            return Ok(categoriesWithActivities);
        }

        [AllowAnonymous]
        [HttpGet]
        [ActionName("by_categories")]
        public IHttpActionResult GetByCategories()
        {
            var categoriesWithActivities = _activity.GetGroupedByCategoriesActivityTemplates();
            return Ok(categoriesWithActivities);
        }

        [ResponseType(typeof(IEnumerable<ActivityTemplateDTO>))]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult Get(string tag)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Func<ActivityTemplateDO, bool> predicate = (at) =>
                    string.IsNullOrEmpty(at.Tags) ? false :
                        at.Tags.Split(new char[] { ',' }).Any(c => string.Equals(c.Trim(), tag, StringComparison.InvariantCultureIgnoreCase));
                var categoriesWithActivities = _activity.GetAvailableActivities(uow, tag == "[all]" ? (at) => true : predicate);
                return Ok(categoriesWithActivities);
            }
        }
    }
}