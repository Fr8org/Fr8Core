using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class SubPlansController : ApiController
    {
        private readonly ISubPlan _subPlan;

        public SubPlansController()
        {
            _subPlan = ObjectFactory.GetInstance<ISubPlan>();
        }

        [ResponseType(typeof(SubPlanDTO))]
        public IHttpActionResult Post(SubPlanDTO subPlanDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(subPlanDTO.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Validation failed for posted Subplan");
                }

                //TODO invalid mappings prevent this line from running
                //fix invalid automapper configurations
                //var curSubPlanDO = Mapper.Map<SubrouteDTO, SubrouteDO>(subPlanDTO);
                var curSubPlanDO = new SubPlanDO(false)
                {
                    Id = Guid.Empty,
                    ParentPlanNodeId = subPlanDTO.ParentId,
                    RootPlanNodeId = subPlanDTO.PlanId,
                    Name = subPlanDTO.Name,
                    Runnable = subPlanDTO.Runnable
                };

                _subPlan.Create(uow, curSubPlanDO);
                uow.SaveChanges();

                return Ok(Mapper.Map<SubPlanDO, SubPlanDTO>(curSubPlanDO));
            }
        }

        [ResponseType(typeof(SubPlanDTO))]
        public IHttpActionResult Put(SubPlanDTO subPlanDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(subPlanDTO.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Validation failed for posted Subplan");
                }
                //TODO invalid mappings prevent this line from running
                //fix invalid automapper configurations
                //var curSubPlanDO = Mapper.Map<SubrouteDTO, SubrouteDO>(subPlanDTO);
                var curSubPlanDO = new SubPlanDO(false)
                {
                    Id = subPlanDTO.SubPlanId.Value,
                    ParentPlanNodeId = subPlanDTO.PlanId,
                    RootPlanNodeId = subPlanDTO.PlanId,
                    Name = subPlanDTO.Name
                };
                _subPlan.Update(uow, curSubPlanDO);
                uow.SaveChanges();
                return Ok(Mapper.Map<SubPlanDO, SubPlanDTO>(curSubPlanDO));
            }
        }

        [ResponseType(typeof(SubPlanDTO))]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subPlan = uow.PlanRepository.GetById<SubPlanDO>(id);
                if (subPlan == null)
                {
                    return BadRequest();
                }

                await _subPlan.Delete(uow, id);

                uow.SaveChanges();

                return Ok();
            }
        }

        [ActionName("first_activity")]
        [ResponseType(typeof(ActivityDTO))]
        [HttpPost]
        public async Task<IHttpActionResult> FirstActivity(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activity = _subPlan.GetFirstActivity(uow, id);
                return Ok(Mapper.Map<ActivityDTO>(activity));
            }
        }
    }
}