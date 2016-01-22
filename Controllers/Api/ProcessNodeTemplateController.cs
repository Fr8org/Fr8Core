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
    /// Subroute web api controller to handle CRUD operations from frontend.
    /// </summary>
    //[RoutePrefix("api/processNodeTemplate")]
    [Fr8ApiAuthorize]
    public class ProcessNodeTemplateController : ApiController
    {
        /// <summary>
        /// Instance of Subroute service.
        /// </summary>
        private readonly ISubroute _subroute;

        public ProcessNodeTemplateController()
        {
            _subroute = ObjectFactory.GetInstance<ISubroute>();
        }

        /// <summary>
        /// Retrieve Subroute by id.
        /// </summary>
        /// <param name="id">Subroute id.</param>
        [ResponseType(typeof(SubrouteDTO))]
        public IHttpActionResult Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subroute = uow.SubrouteRepository.GetByKey(id);

                return Ok(Mapper.Map<SubrouteDTO>(subroute));
            };
        }

        /// <summary>
        /// Recieve Subroute with temporary id, create Subroute,
        /// create Criteria, and return DTO for Subroute with global id.
        /// </summary>
        /// <param name="dto">Subroute data transfer object.</param>
        /// <returns>Created Subroute with global id.</returns>
        public IHttpActionResult Post(SubrouteDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subroute = Mapper.Map<SubrouteDO>(dto);
                _subroute.Store(uow, subroute);

                uow.SaveChanges();

                return Ok(new { Id = subroute.Id });
            }
        }

        /// <summary>
        /// Recieve Subroute with global id, update,
        /// and return DTO for updated entity.
        /// </summary>
        /// <param name="dto">Subroute data transfer object.</param>
        /// <returns>Updated Subroute.</returns>
        [ResponseType(typeof(SubrouteDTO))]
        [HttpPut]
        public IHttpActionResult Update(SubrouteDTO dto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subroute = Mapper.Map<SubrouteDO>(dto);
                _subroute.Update(uow, subroute);

                uow.SaveChanges();

                return Ok(Mapper.Map<SubrouteDTO>(subroute));
            }
        }

        /// <summary>
        /// Delete Subroute by id.
        /// </summary>
        /// <param name="id">Subroute id.</param>
        /// <returns>Deleted DTO Subroute.</returns>
        [ResponseType(typeof(SubrouteDTO))]
        public IHttpActionResult Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _subroute.Delete(uow, id);

                return Ok();
            }
        }
    }
}