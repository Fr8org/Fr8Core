using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [RoutePrefix("activityTemplates")]
    public class ActivityTemplatesController : ApiController
    {
        private readonly IActivityTemplate _activityTemplateService;

        public ActivityTemplatesController()
        {
            _activityTemplateService = ObjectFactory.GetInstance<IActivityTemplate>();
        }

        [HttpGet]
        [ResponseType(typeof(ActivityTemplateDTO))]
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
    }
}