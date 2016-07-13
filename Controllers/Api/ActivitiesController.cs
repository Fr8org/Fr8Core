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
using System.Web.Http.Description;

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
        /// <summary>
        /// Creates new activity with specified parameters
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <param name="activityTemplateId">Activity template Id</param>
        /// <param name="label">Label to use in activity header</param>
        /// <param name="name">Name of the plan being created. If parentNodeId parameter is specified then this parameter is ignored</param>
        /// <param name="order">Position inside parent plan. If not specified then newly created activity is placed at the end of plan</param>
        /// <param name="parentNodeId">Id of plan to add activity to. If not specified then new plan will be created and set as parent</param>
        /// <param name="authorizationTokenId">Id of authorization token to grant to the new activity. Can be empty</param>
        /// <response code="200">Activity was succesfully created</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Specified plan is in running state and activity can't be added to it</response>
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
                var result = await _activityService.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, !parentNodeId.HasValue, authorizationTokenId) as ActivityDO;
                return Ok(Mapper.Map<ActivityDTO>(result));
            }
        }
        /// <summary>
        /// Performs configuration of specified activity and returns updated instance of this activity        
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided. <br/>
        /// Callers to this endpoint expect to receive back what they need to know to encode user configuration data into the Action. The typical scenario involves a front-end client calling this and receiving back the same Action they passed, but with an attached Configuration Crate. The client renders UI based on the Configuration Crate, collects user inputs, and saves them as values in the Configuration Crate JSON. The updated Configuration Crate is then saved to the server so it will be available to the processing Terminal at run-time.
        /// </remarks>
        /// <param name="curActionDesignDTO">Activity to configure</param>
        /// <param name="force">True (1) to force updting activity that belong to plan that is currently in running state. Otherwise activity belong or being added to running plan won't be saved</param>
        /// <response code="200">Configured activity</response>
        /// <response code="400">Activity is not specified or doesn't exist</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Owning plan is in running state and activity can't be changed and force flag is not overriden</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO, [FromUri]bool force = false)
        {
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
        /// Returns an activity with the specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Retrieved activity</response>
        /// <response code="400">Activity doesn't exist</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [ResponseType(typeof(ActivityDTO))]
        public IHttpActionResult Get(Guid id)
        {
            if (!_activityService.Exists(id))
            {
                return BadRequest("Activity doesn't exist");
            }
            using (var uow = _uowFactory.Create())
            {
                return Ok(Mapper.Map<ActivityDTO>(_activityService.GetById(uow, id)));
            }
        }

        /// <summary>
        /// Deletes activity with specified Id. If 'deleteChildNodes' flag is specified, only deletes child activities of specified activity
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Activity was successfully deleted</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Owning plan is in running state and activity can't be changed</response>
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
        /// Updates activity if one with specified Id exists. Otherwise creates a new activity
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="curActionDTO">Activity data to save</param>
        /// <param name="force">True (1) to force updting activity that belong to plan that is currently in running state. Otherwise activity belong or being added to running plan won't be saved</param>
        /// <response code="200">Newly created or updated activity</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Owning plan is in running state and activity can't be changed and force flag is not overriden</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
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