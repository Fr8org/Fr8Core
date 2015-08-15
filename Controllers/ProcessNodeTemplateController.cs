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
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// ProcessNodeTemplate web api controller to handle CRUD operations from frontend.
    /// </summary>
    [RoutePrefix("api/processNodeTemplate")]
    public class ProcessNodeTemplateController : ApiController
    {
        /// <summary>
        /// Instance of ProcessNodeTemplate service.
        /// </summary>
        private readonly IProcessNodeTemplate _processNodeTemplate;

        public ProcessNodeTemplateController()
        {
            _processNodeTemplate = ObjectFactory.GetInstance<IProcessNodeTemplate>();
        }

        /// <summary>
        /// Retrieve criteria by ProcessNodeTemplate.Id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate.id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        [Route("criteria")]
        [HttpGet]
        public IHttpActionResult GetByProcessNodeTemplateId(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(id);
                var curCriteria = curProcessNodeTemplate.Criteria;

                return Ok(Mapper.Map<CriteriaDTO>(curCriteria));
            };
        }

        /// <summary>
        /// Retrieve ProcessNodeTemplate by id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate id.</param>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(id);

                return Ok(Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate));
            };
        }

        /// <summary>
        /// Recieve ProcessNodeTemplate with temporary id, create ProcessNodeTemplate,
        /// create Criteria, and return DTO for ProcessNodeTemplate with global id.
        /// </summary>
        /// <param name="dto">ProcessNodeTemplate data transfer object.</param>
        /// <returns>Created ProcessNodeTemplate with global id.</returns>
        [ResponseType(typeof(int))]
        public IHttpActionResult Post(ProcessNodeTemplateDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processNodeTemplate = Mapper.Map<ProcessNodeTemplateDO>(dto);
                _processNodeTemplate.Create(uow, processNodeTemplate);

                uow.SaveChanges();

                return Ok(processNodeTemplate.Id);
            }
        }

        /// <summary>
        /// Recieve ProcessNodeTemplate with global id, update,
        /// and return DTO for updated entity.
        /// </summary>
        /// <param name="dto">ProcessNodeTemplate data transfer object.</param>
        /// <returns>Updated ProcessNodeTemplate.</returns>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        [HttpPut]
        public IHttpActionResult Update(ProcessNodeTemplateDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processNodeTemplate = Mapper.Map<ProcessNodeTemplateDO>(dto);
                _processNodeTemplate.Update(uow, processNodeTemplate);

                uow.SaveChanges();

                return Ok(Mapper.Map<ProcessNodeTemplateDTO>(processNodeTemplate));
            }
        }

        /// <summary>
        /// Delete ProcessNodeTemplate by id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate id.</param>
        /// <returns>Deleted DTO ProcessNodeTemplate.</returns>
        [ResponseType(typeof(ProcessNodeTemplateDTO))]
        public IHttpActionResult Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _processNodeTemplate.Delete(uow, id);

                return Ok();
            }
        }
    }
}