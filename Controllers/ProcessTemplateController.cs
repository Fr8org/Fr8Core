using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Ajax.Utilities;
using StructureMap;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Web.Controllers.Helpers;
using Web.ViewModels;

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

        [Route("full")]
        [ResponseType(typeof(FullProcessTemplateDTO))]
        [HttpGet]
        public IHttpActionResult GetFullProcessTemplate(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = uow.ProcessTemplateRepository.GetByKey(id);
                var result = Mapper.Map<FullProcessTemplateDTO>(processTemplate,
                    opts => { opts.Items[ProcessTemplateDOFullConverter.UnitOfWork_OptionsKey] = uow; });

                return Ok(result);
            };
        }

        // GET api/<controller>
        public IHttpActionResult Get(int? id = null)
        {
            var curProcessTemplates = _processTemplate.GetForUser(User.Identity.Name, User.IsInRole(Roles.Admin),id);

            if (curProcessTemplates.Any())
            {
                // Return first record from curProcessTemplates, in case id parameter was provided.
                // User intentionally wants to receive a single JSON object in response.
                if (id.HasValue)
                {
                    return Ok(Mapper.Map<ProcessTemplateDTO>(curProcessTemplates.First()));
                }

                // Return JSON array of objects, in case no id parameter was provided.
                return Ok(curProcessTemplates.Select(Mapper.Map<ProcessTemplateDTO>));
            }

            //DO-840 Return empty view as having empty process templates are valid use case.
            return Ok();
        }

        public IHttpActionResult Post(ProcessTemplateDTO processTemplateDto)
        {

            if (string.IsNullOrEmpty(processTemplateDto.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the request data is invalid");
            }

            var curProcessTemplateDO = Mapper.Map<ProcessTemplateDTO, ProcessTemplateDO>(processTemplateDto);
            curProcessTemplateDO.UserId = User.Identity.Name;
            processTemplateDto.Id = _processTemplate.CreateOrUpdate(curProcessTemplateDO);

            return Ok(processTemplateDto);
        }

        public IHttpActionResult Delete(int id)
        {
            _processTemplate.Delete(id);
            return Ok(id);
        }
    }
}