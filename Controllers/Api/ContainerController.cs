using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using HubWeb.Infrastructure;
using StructureMap;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalInterface = Hub.Interfaces;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8Data.DataTransferObjects;
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

        [Fr8ApiAuthorize]
        [HttpGet]
        public IHttpActionResult GetIdsByName(string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerIds = uow.ContainerRepository.GetQuery().Where(x => x.Name == name).Select(x => x.Id).ToArray();

                return Json(containerIds);
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

        //private DockyardDbContext db = new DockyardDbContext();
        // GET: api/Process
        //public IQueryable<ProcessDO> Get()
        //{
        //    return db.Processes;
        //}

        //// GET: api/Process/5
        //[ResponseType(typeof(ProcessDO))]
        //public IHttpActionResult GetProcess(int id)
        //{
        //    ProcessDO processDO = db.Processes.Find(id);
        //    if (processDO == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(processDO);
        //}

        //// PUT: api/Process/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutProcess(int id, ProcessDO processDO)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != processDO.Id)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(processDO).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProcessDOExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/Process
        //[ResponseType(typeof(ProcessDO))]
        //public IHttpActionResult PostProcessDO(ProcessDO processDO)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Processes.Add(processDO);
        //    db.SaveChanges();

        //    return CreatedAtPlan("DefaultApi", new { id = processDO.Id }, processDO);
        //}

        //// DELETE: api/Process/5
        //[ResponseType(typeof(ProcessDO))]
        //public IHttpActionResult DeleteProcessDO(int id)
        //{
        //    ProcessDO processDO = db.Processes.Find(id);
        //    if (processDO == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Processes.Remove(processDO);
        //    db.SaveChanges();

        //    return Ok(processDO);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool ProcessDOExists(int id)
        //{
        //    return db.Processes.Count(e => e.Id == id) > 0;
        //}
    }
}