using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// ProcessNodeTemplate web api controller to handle CRUD operations from frontend.
    /// </summary>
    [RoutePrefix("api/processNodeTemplate")]
    public class ProcessNodeTemplateController : ApiController, IUnitOfWorkAwareComponent
    {
        /// <summary>
        /// Instance of ProcessNodeTemplate service.
        /// </summary>
        public IProcessNodeTemplate ProcessNodeTemplateService
        {
            get { return ObjectFactory.GetInstance<IProcessNodeTemplate>(); }
        }

        /// <summary>
        /// Retrieve ProcessNodeTemplate by id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate id.</param>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Get(int id)
        {
            return this.InUnitOfWork(uow =>
            {
                var processNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(id);
                return Ok(Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate));
            });
        }

        /// <summary>
        /// Recieve ProcessNodeTemplate with temporary id, create ProcessNodeTemplate,
        /// create Criteria, and return DTO for ProcessNodeTemplate with global id.
        /// </summary>
        /// <param name="dto">ProcessNodeTemplate data transfer object.</param>
        /// <returns>Created ProcessNodeTemplate with global id.</returns>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Post(ProcessNodeTemplateDTO dto)
        {
            var processNodeTemplate = Mapper.Map<ProcessNodeTemplateDO>(dto);
            ProcessNodeTemplateService.Create(processNodeTemplate);

            return Ok(Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate));
        }

        /// <summary>
        /// Recieve ProcessNodeTemplate with global id, update,
        /// and return DTO for updated entity.
        /// </summary>
        /// <param name="dto">ProcessNodeTemplate data transfer object.</param>
        /// <returns>Updated ProcessNodeTemplate.</returns>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Put(ProcessNodeTemplateDTO dto)
        {
            var processNodeTemplate = Mapper.Map<ProcessNodeTemplateDO>(dto);
            ProcessNodeTemplateService.Update(processNodeTemplate);

            return Ok(Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate));
        }

        /// <summary>
        /// Delete ProcessNodeTemplate by id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate id.</param>
        /// <returns>Deleted DTO ProcessNodeTemplate.</returns>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Delete(int id)
        {
            var processNodeTemplate = ProcessNodeTemplateService.Remove(id);
            var dto = Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate);

            return Ok(dto);
        }
    }
}