using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    /// <summary>
    /// Critera web api controller to handle operations from frontend.
    /// </summary>
    [RoutePrefix("api/criteria")]
    public class CriteriaController : ApiController
    {
        /// <summary>
        /// Retrieve criteria by ProcessNodeTemplate.Id.
        /// </summary>
        /// <param name="id">ProcessNodeTemplate.id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        [Route("byProcessNodeTemplate")]
        [HttpGet]
        public IHttpActionResult GetByProcessNodeTemplateId(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curCriteria = uow.CriteriaRepository.GetQuery()
                    .SingleOrDefault(x => x.ProcessNodeTemplateId == id);

                return Ok(Mapper.Map<CriteriaDTO>(curCriteria));
            };
        }

        /// <summary>
        /// Recieve criteria with global id, update criteria,
        /// and return updated criteria.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Updated criteria.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        [HttpPut]
        public IHttpActionResult Update(CriteriaDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                CriteriaDO curCriteria = null;

                curCriteria = uow.CriteriaRepository.GetByKey(dto.Id);
                if (curCriteria == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", dto.Id));
                }

                Mapper.Map<CriteriaDTO, CriteriaDO>(dto, curCriteria);

                uow.SaveChanges();

                return Ok(Mapper.Map<CriteriaDTO>(curCriteria));
            };
        }
    }
}