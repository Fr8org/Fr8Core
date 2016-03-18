using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    /// <summary>
    /// SubPlan web api controller to handle CRUD operations from frontend.
    /// </summary>
    //[RoutePrefix("api/processNodeTemplate")]
    [Fr8ApiAuthorize]
    public class ProcessNodeTemplateController : ApiController
    {
        /// <summary>
        /// Instance of SubPlan service.
        /// </summary>
        private readonly ISubPlan _subPlan;

        public ProcessNodeTemplateController()
        {
            _subPlan = ObjectFactory.GetInstance<ISubPlan>();
        }

        //        /// <summary>
        //        /// Retrieve SubPlan by id.
        //        /// </summary>
        //        /// <param name="id">SubPlan id.</param>
        //        [ResponseType(typeof(SubPlanDTO))]
        //        public IHttpActionResult Get(int id)
        //        {
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var subPlan = uow.PlanRepository.GetById<SubPlanDO>(id);
        //
        //                return Ok(Mapper.Map<SubPlanDTO>(subPlan));
        //            };
        //        }

        /// <summary>
        /// Recieve SubPlan with temporary id, create SubPlan,
        /// create Criteria, and return DTO for SubPlan with global id.
        /// </summary>
        /// <param name="dto">SubrPlan data transfer object.</param>
        /// <returns>Created SubPlan with global id.</returns>
        public IHttpActionResult Post(SubPlanDTO dto)
        {
            // do we need this anymore?
            throw new NotImplementedException();
         
        }

        /// <summary>
        /// Recieve SubPlan with global id, update,
        /// and return DTO for updated entity.
        /// </summary>
        /// <param name="dto"> data transfer object.</param>
        /// <returns>Updated SubPlan.</returns>
        [ResponseType(typeof(SubPlanDTO))]
        [HttpPut]
        public IHttpActionResult Update(SubPlanDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subPlan = Mapper.Map<SubPlanDO>(dto);

                _subPlan.Update(uow, subPlan);

                uow.SaveChanges();

                return Ok(Mapper.Map<SubPlanDTO>(subPlan));
            }
        }

        /// <summary>
        /// Delete SubPlan by id.
        /// </summary>
        /// <param name="id">SubPlan id.</param>
        /// <returns>Deleted DTO SubPlan.</returns>
        [ResponseType(typeof(SubPlanDTO))]
        public IHttpActionResult Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _subPlan.Delete(uow, id);

                return Ok();
            }
        }
    }
}