using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Fr8.Infrastructure;
using Swashbuckle.Swagger.Annotations;

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
        [HttpGet]
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [ActionName("payload")]
        [SwaggerResponse(HttpStatusCode.OK, "Container's crate storage", typeof(PayloadDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Container doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public IHttpActionResult GetPayload(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var container = uow.ContainerRepository.GetByKey(id);
                if (container == null)
                {
                    throw new MissingObjectException($"Container with Id {id} doesn't exist");
                }
                var payload = new PayloadDTO(id);
                if (container.CrateStorage == null)
                {
                    container.CrateStorage = string.Empty;
                }
                payload.CrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(container.CrateStorage);
                return Ok(payload);
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
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Container with specified Id. Can be empty", typeof(ContainerDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Container doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
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

        /// <summary>
        /// Retrieves all containers belong to plans of current user that satisfy specified critiera. Pagination is applied to the result set
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ActionName("query")]
        [ResponseType(typeof(IEnumerable<ContainerDTO>))]
        [SwaggerResponse(HttpStatusCode.OK, "Subset of containers satisfying specified query", typeof(PagedResultDTO<ContainerDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request")]
        public IHttpActionResult GetByQuery([FromUri]PagedQueryDTO pagedQuery)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var results = _containerService.GetByQuery(_security.GetCurrentAccount(uow), pagedQuery);
                return Ok(results);
            }
        }

        //NOTE: IF AND WHEN THIS CLASS GETS USED, IT NEEDS TO BE FIXED TO USE OUR 
        //STANDARD UOW APPROACH, AND NOT CONTACT THE DATABASE TABLE DIRECTLY.
    }
}