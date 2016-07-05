using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
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
        /// Performs configuration of specified activity and returns updated instance of this activity        
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided. <br/>
        /// Callers to this endpoint expect to receive back what they need to know to encode user configuration data into the Action. The typical scenario involves a front-end client calling this and receiving back the same Action they passed, but with an attached Configuration Crate. The client renders UI based on the Configuration Crate, collects user inputs, and saves them as values in the Configuration Crate JSON. The updated Configuration Crate is then saved to the server so it will be available to the processing Terminal at run-time.
        /// </remarks>
        /// <param name="curActionDesignDTO">Activity to configure</param>
        /// <response code="200">Configured activity</response>
        /// <response code="400">Activity is not specified or doesn't exist</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Owning plan is in running state and activity can't be changed</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO)
        {
            curActionDesignDTO.CurrentView = null;
            ActivityDO curActivityDO = Mapper.Map<ActivityDO>(curActionDesignDTO);
            var userId = User.Identity.GetUserId();
            using (var uow = _uowFactory.Create())
            {
                if (_planService.GetPlanState(uow, curActionDesignDTO.Id) == PlanState.Running)
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
        /// <response code="200">Newly created or updated activity</response>
        /// <response code="403">Unauthorized request</response>
        /// <response code="423">Owning plan is in running state and activity can't be changed</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO)
        {
            using (var uow = _uowFactory.Create())
            {
                if (_planService.GetPlanState(uow, curActionDTO.Id) == PlanState.Running)
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