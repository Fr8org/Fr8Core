using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using HubWeb.Infrastructure;
using Microsoft.AspNet.Identity;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalInterface = Hub.Interfaces;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;

namespace HubWeb.Controllers
{
    // commented out by yakov.gnusin.
    // Please DO NOT put [Fr8ApiAuthorize] on class, this breaks process execution!
    // [Fr8ApiAuthorize]
    public class ContainersController : ApiController
    {
        private readonly InternalInterface.IContainer _container;
        private readonly ISecurityServices _security;

        public ContainersController()
        {
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        [HttpGet]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        [ActionName("payload")]
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

        // Return the Containers accordingly to ID given
        [Fr8ApiAuthorize]
        //[Route("get/{id:guid?}")]
        [HttpGet]
        public IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IList<ContainerDO> curContainer = _container
                    .GetByFr8Account(
                        uow,
                        _security.GetCurrentAccount(uow),
                        _security.IsCurrentUserHasRole(Roles.Admin),
                        id
                    );

                if (curContainer.Any())
                {
                    if (id.HasValue)
                    {
                        return Ok(Mapper.Map<ContainerDTO>(curContainer.First()));
                    }

                    return Ok(curContainer.Select(Mapper.Map<ContainerDTO>));
                }
                return Ok();
            }
        }
      
        //NOTE: IF AND WHEN THIS CLASS GETS USED, IT NEEDS TO BE FIXED TO USE OUR 
        //STANDARD UOW APPROACH, AND NOT CONTACT THE DATABASE TABLE DIRECTLY.
    }
}