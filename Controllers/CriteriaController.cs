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
    /// Critera web api controller to handle CRUD operations from frontend.
    /// </summary>
    [RoutePrefix("api/criteria")]
    public class CriteriaController : ApiController, IUnitOfWorkAwareComponent
    {
        /// <summary>
        /// Retrieve all criteria by ProcessTemplate identity.
        /// </summary>
        /// <returns>List of criteria.</returns>
        [Route("all")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<CriteriaDTO>))]
        public IHttpActionResult All(int processTemplateId)
        {
            return this.InUnitOfWork(uow =>
            {
                var data = uow.CriteriaRepository
                    .GetQuery()
                    .Where(x => x.ProcessTemplateId == processTemplateId)
                    .OrderBy(x => x.Id)
                    .AsEnumerable()
                    .Select(x => Mapper.Map<CriteriaDTO>(x))
                    .ToList();

                return Ok(data);
            });
        }

        /// <summary>
        /// Retrieve criteria by id.
        /// </summary>
        /// <param name="id">Criteria id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Get(int id)
        {
            return this.InUnitOfWork(uow =>
            {
                var criteria = uow.CriteriaRepository.GetByKey(id);
                return Ok(Mapper.Map<CriteriaDTO>(criteria));
            });
        }

        /// <summary>
        /// Recieve criteria with temporary id, create criteria,
        /// and return criteria with global id.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Created criteria with global id.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Post(CriteriaDTO dto)
        {
            var criteria = Mapper.Map<CriteriaDO>(dto);

            this.InUnitOfWork(uow =>
            {
                uow.CriteriaRepository.Add(criteria);
            });

            var resultDTO = Mapper.Map<CriteriaDTO>(criteria);
            return Ok(resultDTO);
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
            CriteriaDO criteria = null;

            this.InUnitOfWork(uow =>
            {
                criteria = uow.CriteriaRepository.GetByKey(dto.Id);
                if (criteria == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", dto.Id));
                }

                Mapper.Map<CriteriaDTO, CriteriaDO>(dto, criteria);
            });

            var resultDTO = Mapper.Map<CriteriaDTO>(criteria);
            return Ok(resultDTO);
        }

        /// <summary>
        /// Delete criteria by id provided.
        /// </summary>
        /// <param name="id">Criteria id.</param>
        /// <returns>Deleted criteria.</returns>
        [ResponseType(typeof(CriteriaDTO))]
        public IHttpActionResult Delete(int id)
        {
            return this.InUnitOfWork(uow =>
            {
                var criteria = uow.CriteriaRepository.GetByKey(id);
                if (criteria == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", id));
                }

                var dto = Mapper.Map<CriteriaDTO>(criteria);

                uow.CriteriaRepository.Remove(criteria);

                return Ok(dto);
            });
        }
    }
}