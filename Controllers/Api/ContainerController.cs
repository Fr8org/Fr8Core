using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Hub.Infrastructure;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using Newtonsoft.Json;
using System.Web.Http.Description;

namespace HubWeb.Controllers
{
    // commented out by yakov.gnusin.
    // Please DO NOT put [Fr8ApiAuthorize] on class, this breaks process execution!
    // [Fr8ApiAuthorize]
    public class ContainersController : ApiController
    {
        private readonly IContainerService _containerService;
        private readonly ISecurityServices _security;

        public ContainersController()
        {
            _containerService = ObjectFactory.GetInstance<IContainerService>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }
        /// <summary>
        /// Retrieves crate storage of the container with specified Id
        /// </summary>
        /// <param name="id">Id of the container</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Container's crate storage</response>
        /// <response code="403">Unathorized request</response>
        [HttpGet]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        [ActionName("payload")]
        [ResponseType(typeof(PayloadDTO))]
        public IHttpActionResult GetPayload(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = uow.ContainerRepository.GetByKey(id);
                var curPayloadDTO = new PayloadDTO(id);

                if (curContainerDO.CrateStorage == null)
                {
                    curContainerDO.CrateStorage = string.Empty;
                }

                curPayloadDTO.CrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(curContainerDO.CrateStorage);

                return Ok(curPayloadDTO);
            }
        }
        /// <summary>
        /// Retrieves all containers belong to plans of current user
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Collection of containers. Can be empty</response>
        /// <response code="403">Unathorized request</response>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<ContainerDTO>))]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IList<ContainerDO> curContainer = _containerService
                    .GetByFr8Account(
                        uow,
                        _security.GetCurrentAccount(uow),
                        _security.IsCurrentUserHasRole(Roles.Admin),
                        null
                    );

                if (curContainer.Any())
                {
                    return Ok(curContainer.Select(Mapper.Map<ContainerDTO>));
                }
                return Ok();
            }
        }

        /// <summary>
        /// Retrieves container with specified Id
        /// </summary>
        /// <param name="id">Id of the container</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Container with specified Id. Can be empty</response>
        /// <response code="403">Unathorized request</response>
        [Fr8ApiAuthorize]
        [HttpGet]
        public IHttpActionResult Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IList<ContainerDO> curContainer = _containerService
                    .GetByFr8Account(
                        uow,
                        _security.GetCurrentAccount(uow),
                        _security.IsCurrentUserHasRole(Roles.Admin),
                        id
                    );

                if (curContainer.Any())
                {
                    return Ok(Mapper.Map<ContainerDTO>(curContainer.First()));
                }
                return Ok();
            }
        }
      
        //NOTE: IF AND WHEN THIS CLASS GETS USED, IT NEEDS TO BE FIXED TO USE OUR 
        //STANDARD UOW APPROACH, AND NOT CONTACT THE DATABASE TABLE DIRECTLY.
    }
}