using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
//
using InternalInterface = Core.Interfaces;
using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Microsoft.AspNet.Identity;

namespace Web.Controllers
{
    [RoutePrefix("api/containers")]
    public class ContainerController : ApiController
    {
        private readonly InternalInterface.IContainer _container;


        public ContainerController()
        {
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = uow.ContainerRepository.GetByKey(id);
                var curPayloadDTO = new PayloadDTO(curContainerDO.CrateStorage, id);

                EventManager.ProcessRequestReceived(curContainerDO);

                return Ok(curPayloadDTO);
            }
        }

        [Route("getIdsByName")]
        [HttpGet]
        public IHttpActionResult GetIdsByName(string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var containerIds = uow.ContainerRepository.GetQuery().Where(x => x.Name == name).Select(x => x.Id).ToArray();

                return Json(containerIds);
            }
        }

        [Route("launch")]
        [HttpPost]
        public async Task<IHttpActionResult> Launch(int routeId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplateDO = uow.RouteRepository.GetByKey(routeId);
                await _container.Launch(processTemplateDO, null);

                return Ok();
            }
        }

        // Return the Containers accordingly to ID given
        [Route("get/{id:int?}")]
        [HttpGet]
        public IHttpActionResult Get(int? id = null)
        {
                IList<ContainerDO> curContainer = _container.GetByDockyardAccount(User.Identity.GetUserId(), User.IsInRole(Roles.Admin), id);

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

        //    return CreatedAtRoute("DefaultApi", new { id = processDO.Id }, processDO);
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