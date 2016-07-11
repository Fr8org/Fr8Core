using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;

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
        /// <response code="200">Subplan was successfully created</response>
        /// <response code="400">Specified data is not valid</response>
        [ResponseType(typeof(SubplanDTO))]
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
                    Runnable = subplanDto.Runnable
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
        /// <response code="200">Subplan was successfully updated</response>
        /// <response code="400">Specified data is not valid</response>
        [ResponseType(typeof(SubplanDTO))]
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
                    Id = subplanDto.SubPlanId.Value,
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
        /// <response code="200">Subplan was successfully deleted</response>
        /// <response code="400">Subplan with specified Id doesn't exist</response>
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subPlan = uow.PlanRepository.GetById<SubplanDO>(id);
                if (subPlan == null)
                {
                    return BadRequest();
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
        /// <remarks>
        /// At this point value of 'filter' parameter should be set to 'first'
        /// </remarks>
        /// <param name="id">Id of subplan</param>
        /// <param name="filter">Only accepts value of 'first', otherwise no data is returned</param>
        /// <response code="200">First activity of the subplan with specified Id. Can be empty</response>
        [ActionName("activities")]
        [ResponseType(typeof(ActivityDTO))]
        [HttpPost]
        public async Task<IHttpActionResult> FirstActivity(Guid id, string filter)
        {
            filter = (filter ?? string.Empty).Trim().ToLower();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (filter == "first")
                {
                    try
                    {
                        var activity = _subplan.GetFirstActivity(uow, id);
                        return Ok(Mapper.Map<ActivityDTO>(activity));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.Message);
                        return InternalServerError(ex);
                    }
                }
                return Ok();
            }
        }
    }
}