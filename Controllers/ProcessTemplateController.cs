using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using StructureMap;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;

namespace Web.Controllers
{
    [Authorize]
    [RoutePrefix("api/processTemplate")]
    public class ProcessTemplateController : ApiController
    {
        private readonly IProcessTemplate _processTemplate;

        public ProcessTemplateController()
            : this(ObjectFactory.GetInstance<IProcessTemplate>())
        {
        }

        public ProcessTemplateController(IProcessTemplate processTemplate)
        {
            _processTemplate = processTemplate;
        }

        [Route("full/{id:int}")]
        [ResponseType(typeof(ProcessTemplateDTO))]
        [HttpGet]
        public IHttpActionResult GetFullProcessTemplate(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = uow.ProcessTemplateRepository.GetByKey(id);

                var test = Mapper.Map<ProcessTemplateOnlyDTO>(processTemplate);

                var result = Mapper.Map<ProcessTemplateDTO>(processTemplate,
                    opts => { opts.Items[ProcessTemplateDOFullConverter.UnitOfWork_OptionsKey] = uow; });

                return Ok(result);
            };
        }

        // GET api/<controller>
        public IHttpActionResult Get(int? id = null)
        {
            var curProcessTemplates = _processTemplate.GetForUser(User.Identity.GetUserId(), User.IsInRole(Roles.Admin),id);

            if (curProcessTemplates.Any())
            {
                // Return first record from curProcessTemplates, in case id parameter was provided.
                // User intentionally wants to receive a single JSON object in response.
                if (id.HasValue)
                {
                    return Ok(Mapper.Map<ProcessTemplateOnlyDTO>(curProcessTemplates.First()));
                }

                // Return JSON array of objects, in case no id parameter was provided.
                return Ok(curProcessTemplates.Select(Mapper.Map<ProcessTemplateOnlyDTO>));
            }

            //DO-840 Return empty view as having empty process templates are valid use case.
            return Ok();
        }

        public IHttpActionResult Post(ProcessTemplateOnlyDTO processTemplateDto, bool updateRegistrations = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(processTemplateDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }

                var curProcessTemplateDO = Mapper.Map<ProcessTemplateOnlyDTO, ProcessTemplateDO>(processTemplateDto, opts => opts.Items.Add("ptid", processTemplateDto.Id));
                var curUserId = User.Identity.GetUserId();
                curProcessTemplateDO.DockyardAccount = uow.UserRepository
                    .GetQuery()
                    .Single(x => x.Id == curUserId);


                //this will return 0 on create operation because of not saved changes
                processTemplateDto.Id = _processTemplate.CreateOrUpdate(uow, curProcessTemplateDO, updateRegistrations);
                uow.SaveChanges();
                //what a mess lets try this
                curProcessTemplateDO.StartingProcessNodeTemplate.ProcessTemplate = curProcessTemplateDO;
                uow.SaveChanges();
                processTemplateDto.Id = curProcessTemplateDO.Id;
                return Ok(processTemplateDto);
            }
        }

        [HttpPost]
        [Route("action")]
        [ActionName("action")]
        public IHttpActionResult PutAction(ActionDTO actionDto)
        {
            //A stub until the functionaltiy is ready
            return Ok();
        }

        public IHttpActionResult Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _processTemplate.Delete(uow, id);

                uow.SaveChanges();
                return Ok(id);
            }
        }

        [Route("triggersettings"), ResponseType(typeof (List<ExternalEventDTO>))]
        public IHttpActionResult GetTriggerSettings()
        {
            var triggerSettings = new List<ExternalEventDTO>()
            {
                new ExternalEventDTO(ExternalEventType.EnvelopeSent, "Envelope Sent"),
                new ExternalEventDTO(ExternalEventType.RecipientDelivered, "Recipient Delivered"),
                new ExternalEventDTO(ExternalEventType.RecipientSent, "Recipient Sent"),
                new ExternalEventDTO(ExternalEventType.RecipientDelivered, "Recipient Received")
            };

            return Ok(triggerSettings);
        }

        [Route("activate")]
        public IHttpActionResult Activate(ProcessTemplateDO curProcessTemplate)
        {
            return Ok(_processTemplate.Activate(curProcessTemplate));
        }

        [Route("deactivate")]
        public IHttpActionResult Deactivate(ProcessTemplateDO curProcessTemplate)
        {
            return Ok(_processTemplate.Deactivate(curProcessTemplate));
        }
    }
}