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
using System;
using System.Data.Entity;

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
                var result = MapProcessTemplateToDTO(processTemplate, uow);

                return Ok(result);
            };
        }

        // Manual mapping method to resolve DO-1164.
        private ProcessTemplateDTO MapProcessTemplateToDTO(ProcessTemplateDO curProcessTemplateDO, IUnitOfWork uow)
        {
            var processNodeTemplateDTOList = uow.ProcessNodeTemplateRepository
                .GetQuery()
                .Include(x => x.ActionLists)
                .Where(x => x.ParentTemplateId == curProcessTemplateDO.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((ProcessNodeTemplateDO x) =>
                {
                    var pntDTO = Mapper.Map<FullProcessNodeTemplateDTO>(x);
                    pntDTO.ActionLists = x.ActionLists.Select(y =>
                    {
                        var actionList = Mapper.Map<FullActionListDTO>(y);
                        actionList.Actions = y.Activities.OfType<ActionDO>()
                                .Select(z => Mapper.Map<ActionDTO>(z))
                                .ToList();
                        return actionList;
                    }).ToList();
                    return pntDTO;
                }).ToList();

            ProcessTemplateDTO result = new ProcessTemplateDTO()
            {
                Description = curProcessTemplateDO.Description,
                Id = curProcessTemplateDO.Id,
                Name = curProcessTemplateDO.Name,
                ProcessTemplateState = curProcessTemplateDO.ProcessTemplateState,
                StartingProcessNodeTemplateId = curProcessTemplateDO.StartingProcessNodeTemplateId,
                ProcessNodeTemplates = processNodeTemplateDTOList
            };

            return result;
        }

        // GET api/<controller>
        public IHttpActionResult Get(int? id = null)
        {
            var curProcessTemplates = _processTemplate.GetForUser(User.Identity.GetUserId(), User.IsInRole(Roles.Admin), id);

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
                _processTemplate.CreateOrUpdate(uow, curProcessTemplateDO, updateRegistrations);
                uow.SaveChanges();
                processTemplateDto.Id = curProcessTemplateDO.Id;
                //what a mess lets try this
                /*curProcessTemplateDO.StartingProcessNodeTemplate.ProcessTemplate = curProcessTemplateDO;
                uow.SaveChanges();
                processTemplateDto.Id = curProcessTemplateDO.Id;*/
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

        [Route("triggersettings"), ResponseType(typeof(List<ExternalEventDTO>))]
        public IHttpActionResult GetTriggerSettings()
        {
            return Ok("This is no longer used due to V2 Event Handling mechanism changes.");
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