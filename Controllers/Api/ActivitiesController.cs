using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Controllers.Api;
using HubWeb.Infrastructure_HubWeb;
using Microsoft.AspNet.Identity;
using StructureMap;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActivitiesController : ApiController
    {
        private readonly IActivity _activityService;
        private readonly IPlan _planService;
        private readonly IUnitOfWorkFactory _uowFactory;

        public ActivitiesController(IActivity activityService, IPlan planService, IUnitOfWorkFactory uowFactory)
        {
            _planService = planService;
            _uowFactory = uowFactory;
            _activityService = activityService;
        }
        
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Create(Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, Guid? authorizationTokenId = null)
        {
            using (var uow = _uowFactory.Create())
            {
                if (parentNodeId != null && _planService.GetPlanState(uow, parentNodeId.Value) == PlanState.Running)
                {
                    return new LockedHttpActionResult();
                }

                var userId = User.Identity.GetUserId();
                var result = await _activityService.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, false, authorizationTokenId) as ActivityDO;
                return Ok(Mapper.Map<ActivityDTO>(result));
            }
        }


        //WARNING. there's lots of potential for confusion between this POST method and the GET method following it.

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO, [FromUri]bool force = false)
        {
            // WebMonitor.Tracer.Monitor.StartMonitoring("Configuring action " + curActionDesignDTO.Name);
            curActionDesignDTO.CurrentView = null;
            ActivityDO curActivityDO = Mapper.Map<ActivityDO>(curActionDesignDTO);
            var userId = User.Identity.GetUserId();
            using (var uow = _uowFactory.Create())
            {
                if (_planService.GetPlanState(uow, curActionDesignDTO.Id) == PlanState.Running && !force)
                {
                    return new LockedHttpActionResult();
                }

                ActivityDTO activityDTO = await _activityService.Configure(uow, userId, curActivityDO);

                return Ok(activityDTO);
            }
        }

        /// <summary>
        /// GET : Returns an action with the specified id
        /// </summary>
        [HttpGet]
        public ActivityDTO Get(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return Mapper.Map<ActivityDTO>(_activityService.GetById(uow, id));
            }
        }

        /// <summary>
        /// DELETE: if flag withChldNodes seted to true Remove all child Nodes and clear activity values
        /// Oterwise delete activity with 'id'
        /// </summary>
        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Delete([FromUri] Guid id, [FromUri(Name = "delete_child_nodes")] bool deleteChildNodes = false)
        {
            using (var uow = _uowFactory.Create())
            {
                if (_planService.GetPlanState(uow, id) == PlanState.Running)
                {
                    return new LockedHttpActionResult();
                }
            }

            if (deleteChildNodes)
            {
                await _activityService.DeleteChildNodes(id);
            }
            else
            {
                await _activityService.Delete(id);
            }

            return Ok();
        }
        
        /// <summary>
        /// POST : Saves or updates the given action
        /// </summary>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO, [FromUri]bool force = false)
        {
            using (var uow = _uowFactory.Create())
            {
                if (_planService.GetPlanState(uow, curActionDTO.Id) == PlanState.Running && !force)
                {
                    return new LockedHttpActionResult();
                }
            }

            var submittedActivityDO = Mapper.Map<ActivityDO>(curActionDTO);

            var resultActionDTO = await _activityService.SaveOrUpdateActivity(submittedActivityDO);

            return Ok(resultActionDTO);
        }
    }
}