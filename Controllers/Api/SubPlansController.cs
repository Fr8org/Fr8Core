using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Hub.Exceptions;
using Hub.Infrastructure;
using HubWeb.Controllers.Helpers;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using System.Threading.Tasks;
using HubWeb.ViewModels;
using Newtonsoft.Json;
using Hub.Managers;
using Data.Crates;
using Data.Interfaces.DataTransferObjects.Helpers;
using Utilities.Interfaces;
using HubWeb.Infrastructure;
using Data.Interfaces.Manifests;
using System.Text;
using Data.Constants;
using Data.Infrastructure;

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
                    ParentPlanNodeId = subPlanDTO.PlanId,
                    RootPlanNodeId = subPlanDTO.PlanId,
                    Name = subPlanDTO.Name
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
    }
}