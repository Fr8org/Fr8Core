using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Infrastructure;
using Hub.Interfaces;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    //[Fr8ApiAuthorize]
    public class SubplansController : ApiController
    {
        private readonly ISubplan _subplan;

        public SubplansController()
        {
            _subplan = ObjectFactory.GetInstance<ISubplan>();
        }
        /// <summary>
        /// Creates new subplan using specified values
        /// </summary>
        /// <param name="subplanDto">Subplan data to create subplan from</param>
        [SwaggerResponse(HttpStatusCode.OK, "Subplan was successfully created", typeof(SubplanDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Parent plan doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public IHttpActionResult Post(SubplanDTO subplanDto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(subplanDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Validation failed for posted Subplan");
                }

                //TODO invalid mappings prevent this line from running
                //fix invalid automapper configurations
                //var curSubPlanDO = Mapper.Map<SubrouteDTO, SubrouteDO>(SubplanDTO);
                var curSubPlanDO = new SubplanDO(false)
                {
                    Id = Guid.Empty,
                    ParentPlanNodeId = subplanDto.ParentId,
                    RootPlanNodeId = subplanDto.PlanId,
                    Name = subplanDto.Name,
                };

                _subplan.Create(uow, curSubPlanDO);
                uow.SaveChanges();

                return Ok(Mapper.Map<SubplanDO, SubplanDTO>(curSubPlanDO));
            }
        }
        /// <summary>
        /// Updates subplan with specified values
        /// </summary>
        /// <param name="subplanDto">Values used to updates subplan</param>
        [SwaggerResponse(HttpStatusCode.OK, "Subplan was successfully updated", typeof(SubplanDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Parent plan doesn't exist or subplan data is invalid", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public IHttpActionResult Put(SubplanDTO subplanDto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(subplanDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Validation failed for posted Subplan");
                }
                //TODO invalid mappings prevent this line from running
                //fix invalid automapper configurations
                //var curSubPlanDO = Mapper.Map<SubrouteDTO, SubrouteDO>(SubplanDTO);
                var curSubPlanDO = new SubplanDO(false)
                {
                    Id = (Guid)subplanDto?.SubPlanId.Value,
                    ParentPlanNodeId = subplanDto.PlanId,
                    RootPlanNodeId = subplanDto.PlanId,
                    Name = subplanDto.Name
                };
                _subplan.Update(uow, curSubPlanDO);
                uow.SaveChanges();
                return Ok(Mapper.Map<SubplanDO, SubplanDTO>(curSubPlanDO));
            }
        }
        /// <summary>
        /// Deletes subplan with specified Id
        /// </summary>
        /// <param name="id">Id of subplan to delete</param>
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK, "Subplan was successfully deleted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Subplan with specified Id doesn't exist")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subPlan = uow.PlanRepository.GetById<SubplanDO>(id);
                if (subPlan == null)
                {
                    throw new MissingObjectException($"Subplan with Id {id} doesn't exist");
                }
                try
                {
                    await _subplan.Delete(uow, id);
                    uow.SaveChanges();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
                return Ok();
            }
        }
        /// <summary>
        /// Retrieves the first activity of the subplan with specified Id
        /// </summary>
        /// <param name="id">Id of subplan</param>
        /// <param name="filter">Deprecated</param>
        /// <response code="200">First activity of the subplan with specified Id. Can be empty</response>
        [ActionName("activities")]
        [ResponseType(typeof(ActivityDTO))]
        [HttpPost]
        public async Task<IHttpActionResult> FirstActivity(Guid id, string filter = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                try
                {
                    var activity = _subplan.GetFirstActivity(uow, id);
                    return Ok(Mapper.Map<ActivityDTO>(activity));
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(ex.Message);
                    return InternalServerError(ex);
                }
            }
        }
    }
}