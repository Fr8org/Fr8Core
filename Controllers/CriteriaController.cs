using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// Critera web api controller to handle operations from frontend.
    /// </summary>
    [RoutePrefix("api/criteria")]
    public class CriteriaController : ApiController, IUnitOfWorkAwareComponent
    {
        /// <summary>
        /// Retrieve criteria by ProcessNodeTemplate.Id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate.id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult GetByProcessNodeTemplateId(int id)
        {
            return this.InUnitOfWork(uow =>
            {
                var processNodeTemplateRepository = uow.ProcessNodeTemplateRepository.GetByKey(id);
                var criteria = processNodeTemplateRepository.Criteria;

                return Ok(Mapper.Map<CriteriaDTO>(criteria));
            });
        }

        /// <summary>
        /// Recieve criteria with global id, update criteria,
        /// and return updated criteria.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Updated criteria.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Put(CriteriaDTO dto)
        {
            return this.InUnitOfWork(uow =>
            {
                CriteriaDO criteria = null;

                criteria = uow.CriteriaRepository.GetByKey(dto.Id);
                if (criteria == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", dto.Id));
                }
            
                Mapper.Map<CriteriaDTO, CriteriaDO>(dto, criteria);

                var resultDTO = Mapper.Map<CriteriaDTO>(criteria);
                return Ok(resultDTO);
            });
        }
    }
}