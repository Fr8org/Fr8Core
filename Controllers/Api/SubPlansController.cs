using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using Utilities.Logging;

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

        [ActionName("activities")]
        [ResponseType(typeof(ActivityDTO))]
        [HttpPost]
        public async Task<IHttpActionResult> FirstActivity(Guid id, string filter)
        {
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