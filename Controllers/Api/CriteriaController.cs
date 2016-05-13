using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using Hub.Infrastructure;
using StructureMap;

namespace HubWeb.Controllers
{
    /// <summary>
    /// Critera web api controller to handle operations from frontend.
    /// </summary>
    //[RoutePrefix("api/criteria")]
    [Fr8ApiAuthorize]
    public class CriteriaController : ApiController
    {
        /// <summary>
        /// Retrieve criteria by Subroute.Id.
        /// </summary>
        /// <param name="id">SubPlan.id.</param>
        [ResponseType(typeof(CriteriaDTO))]
        //[Route("bySubPlan")]
        [HttpGet]
        public IHttpActionResult BySubPlanId(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curCriteria = uow.CriteriaRepository.GetQuery()
                    .SingleOrDefault(x => x.SubPlanId == id);

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