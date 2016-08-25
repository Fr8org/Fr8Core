using System;
using System.Net;
using System.Threading.Tasks;
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
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActivitiesController : ApiController
    {
        private readonly IActivity _activityService;
        private readonly IActivityTemplate _activityTemplateService;
        private readonly IPlan _planService;
        private readonly IUnitOfWorkFactory _uowFactory;

        public ActivitiesController(IActivity activityService, IActivityTemplate activityTemplateService, IPlan planService, IUnitOfWorkFactory uowFactory)
        {
            _planService = planService;
            _uowFactory = uowFactory;
            _activityService = activityService;
            _activityTemplateService = activityTemplateService;
        }
        /// <summary>
        /// Creates an instance of activity from activity template optionally providing necessary authorization
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <param name="activityTemplateId">Id of the activity template of the activity instance that will be created</param>
        /// <param name="label">Label that will be shown in activity header</param>
        /// <param name="name">Name of the plan being created. If parentNodeId parameter is specified then this parameter is ignored</param>
        /// <param name="order">Position inside parent plan. If not specified then newly created activity is placed at the end of plan</param>
        /// <param name="parentNodeId">Id of plan or parent activity to add new activity to. If not specified then new plan will be created and set as parent</param>
        /// <param name="authorizationTokenId">Id of authorization token to grant to the new activity. Can be empty</param>
        [HttpPost]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Activity was succesfully created", typeof(ActivityDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Activity template doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponse((HttpStatusCode)423, "Specified plan is in running state and activity can't be added to it")]
        public async Task<IHttpActionResult> Create(Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, Guid? authorizationTokenId = null)
        {
            using (var uow = _uowFactory.Create())
            {
                if (parentNodeId != null && _planService.IsPlanActiveOrExecuting(parentNodeId.Value))
                {
                    return new LockedHttpActionResult();
                }
                if (!_activityTemplateService.Exists(activityTemplateId))
                {
                    return BadRequest("Activity template doesn't exist");
                };
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
        /// <param name="force">True (1) to force updating activity that belong to plan that is currently in running state. Otherwise activity that belongs to or being added to running plan won't be saved</param>
        [HttpPost]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Activity was successfully configured", typeof(ActivityDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Specifieid activity, activity template or parent plan doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponse((HttpStatusCode)423, "Specified plan is in running state and 'force' flag is not set so activity can't be configured")]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO, [FromUri]bool force = false)
        {
            ActivityDO curActivityDO = Mapper.Map<ActivityDO>(curActionDesignDTO);
            var userId = User.Identity.GetUserId();
            using (var uow = _uowFactory.Create())
            {
                if (_planService.IsPlanActiveOrExecuting(curActionDesignDTO.Id) && !force)
                {
                    return new LockedHttpActionResult();
                }
                var configuredActivity = await _activityService.Configure(uow, userId, curActivityDO);
                return Ok(configuredActivity);
            }
        }

        /// <summary>
        /// Returns an activity with the specified Id
        /// </summary>
        /// <param name="id">Id of activity to retrieve</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "Activity with specified Id", typeof(ActivityDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Activity with specified Id doesn't exist", typeof(DetailedMessageDTO))]
        public IHttpActionResult Get(Guid id)
        {
            if (!_activityService.Exists(id))
            {
                return NotFound();
            }
            using (var uow = _uowFactory.Create())
            {
                return Ok(Mapper.Map<ActivityDTO>(_activityService.GetById(uow, id)));
            }
        }

        /// <summary>
        /// Deletes activity with specified Id. If 'deleteChildNodes' flag is specified, only deletes child activities of specified activity
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided. <br />
        /// Deleting of activity will make downstream activities to reconfigure which may leave them in an invalid state if they had dependency on results produced by activity being deleted
        /// </remarks>
        /// <param name="id">Id of activity to delete</param>
        /// <param name="delete_child_nodes">True to delete child activities only. Otherwise false</param>
        [HttpDelete]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Activity was successfully deleted")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Activity doesn't exist", typeof(DetailedMessageDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponse((HttpStatusCode)423, "Owning plan is in running state so activity can\'t be deleted")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Delete([FromUri] Guid id, [FromUri(Name = "delete_child_nodes")] bool deleteChildNodes = false)
        {
            if (_planService.IsPlanActiveOrExecuting(id))
            {
                return new LockedHttpActionResult();
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
        [HttpPost]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Activity was successfully created or updated")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Activity template or parent plan don't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponse((HttpStatusCode)423, "Owning plan is in running state and 'force' flag is not set so activity can\'t be changed or added to plan")]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO, [FromUri]bool force = false)
        {
            using (var uow = _uowFactory.Create())
            {
                if (_planService.IsPlanActiveOrExecuting(curActionDTO.Id) && !force)
                {
                    return new LockedHttpActionResult();
                }
            }
            var submittedActivity = Mapper.Map<ActivityDO>(curActionDTO);
            var savedActivity = await _activityService.SaveOrUpdateActivity(submittedActivity);
            return Ok(savedActivity);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Subordinate(Guid id)
        {
            using (var uow = _uowFactory.Create())
            {
                var subordinateActivityDO = await _activityService.GetSubordinateActivity(uow, id);
                var subordinateActivityDTO = Mapper.Map<ActivityDTO>(subordinateActivityDO);

                return Ok(subordinateActivityDTO);
            }
        }
    }
}