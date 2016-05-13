using System;
using System.Web.Http;
using AutoMapper;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using StructureMap;

namespace HubWeb.Controllers
{
    public class ActivityTemplatesController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = uow.ActivityTemplateRepository.GetByKey(id);
                return Ok(Mapper.Map<ActivityTemplateDTO>(activityTemplate));
            }
        }
    }
}