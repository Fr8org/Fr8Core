using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Microsoft.AspNet.Identity;
using StructureMap;
using System;

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
                .Include(x => x.Activities)
                .Where(x => x.ParentActivityId == curProcessTemplateDO.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((ProcessNodeTemplateDO x) =>
                {
                    var pntDTO = Mapper.Map<FullProcessNodeTemplateDTO>(x);

                    pntDTO.Actions = Enumerable.ToList(x.Activities.Select(Mapper.Map<ActionDTO>));
                    
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

        
        [Route("getactive")]
        [HttpGet]
        public IHttpActionResult GetByStatus(int? id = null, int? status = null)
        {
            var curProcessTemplates = _processTemplate.GetForUser(User.Identity.GetUserId(), User.IsInRole(Roles.Admin), id, status);

            if (curProcessTemplates.Any())
            {               
                return Ok(curProcessTemplates.Select(Mapper.Map<ProcessTemplateOnlyDTO>));
            }
            return Ok();
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

        [Route("generalSearch")]
        [HttpGet]
        public IHttpActionResult generalSearch(string objtype = "", string id = "", string idoperator = "", string idvalue = "", string createddate = "", string dateoperator = "", string datevalue = "")
        {
            var curProcessTemplates = _processTemplate.GetSearchResultsForUser(objtype, id, idoperator, idvalue, createddate, dateoperator, datevalue);
            int actionid = Convert.ToInt32(idvalue);
            if (curProcessTemplates.Any())
            {

                if (objtype == "02")
                {

                    DateTime cdate = Convert.ToDateTime(datevalue);
                    cdate = cdate.Date;

                    if (idoperator == "gt" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gte" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lt" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lte" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date <= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "eq" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "neq" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }

                    if (idoperator == "gt" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gt" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gt" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid && p.CreatedDate.Date <= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "gt" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid && p.CreatedDate.Date == cdate).ToList();

                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "gt" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }

                    if (idoperator == "gte" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gte" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gte" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date <= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "gte" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "gte" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }


                    if (idoperator == "lt" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lt" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lt" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date <= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lt" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lt" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lte" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lte" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lte" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lte" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lte" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }


                    if (idoperator == "eq" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "eq" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "eq" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "eq" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "eq" && dateoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "neq" && dateoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date > cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "neq" && dateoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date >= cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "neq" && dateoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date < cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "neq" && dateoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date == cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "neq" && dateoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid && p.CreatedDate.Date != cdate).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                }
                else
                {


                    if (idoperator == "gt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id > actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "gte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id >= actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "lt")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id < actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "lte")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id <= actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));
                    }
                    if (idoperator == "eq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id == actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                    if (idoperator == "neq")
                    {
                        curProcessTemplates = curProcessTemplates.Where(p => p.Id != actionid).ToList();
                        return Ok(curProcessTemplates.Select(Mapper.Map<GeneralSearchDO>));

                    }
                }
            }
            return Ok();
        }
        
    }
}