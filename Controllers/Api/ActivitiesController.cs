using System;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using Microsoft.AspNet.Identity;
using StructureMap;
using System.Web.Http.Description;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class ActivitiesController : ApiController
    {
        private readonly IActivity _activity;
        private readonly ITerminal _terminal;

        public ActivitiesController()
        {
            _activity = ObjectFactory.GetInstance<IActivity>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        public ActivitiesController(IActivity service)
        {
            _activity = service;
        }
        /// <summary>
        /// Performs configuration of specified activity and returns updated instance of this activity        
        /// </summary>
        /// <remarks>
        /// Callers to this endpoint expect to receive back what they need to know to encode user configuration data into the Action. The typical scenario involves a front-end client calling this and receiving back the same Action they passed, but with an attached Configuration Crate. The client renders UI based on the Configuration Crate, collects user inputs, and saves them as values in the Configuration Crate JSON. The updated Configuration Crate is then saved to the server so it will be available to the processing Terminal at run-time.
        /// </remarks>
        /// <param name="curActionDesignDTO">Activity to configure</param>
        /// <response code="200">Configured activity</response>
        /// <response code="400">Activity is not specified or doesn't exist</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
        public async Task<IHttpActionResult> Configure(ActivityDTO curActionDesignDTO)
        {
            if (curActionDesignDTO == null || curActionDesignDTO?.Id == Guid.Empty)
            {
                return BadRequest("Empty activity can't be configured");
            }
            if (!_activity.Exists(curActionDesignDTO.Id))
            {
                return BadRequest("Activity doesn't exist");
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {               
                curActionDesignDTO.CurrentView = null;
                ActivityDO curActivityDO = Mapper.Map<ActivityDO>(curActionDesignDTO);
                var userId = User.Identity.GetUserId();
                ActivityDTO activityDTO = await _activity.Configure(uow, userId, curActivityDO);
                return Ok(activityDTO);
            }
        }

        /// <summary>
        /// Returns an activity with the specified Id
        /// </summary>
        /// <response code="200">Retrieved activity</response>
        /// <response code="400">Activity doesn't exist</response>
        [HttpGet]
        [ResponseType(typeof(ActivityDTO))]
        public IHttpActionResult Get(Guid id)
        {
            if (!_activity.Exists(id))
            {
                return BadRequest("Activity doesn't exist");
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return Ok(Mapper.Map<ActivityDTO>(_activity.GetById(uow, id)));
            }
        }

        /// <summary>
        /// Deletes activity with specified Id. If 'deleteChildNodes' flag is specified, only deletes child activities of specified activity
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Activity was successfully deleted</response>
        /// <response code="403">Unauthorized request</response>
        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Delete([FromUri] Guid id, [FromUri(Name = "delete_child_nodes")] bool deleteChildNodes = false)
        {
            if (deleteChildNodes)
            {
                await _activity.DeleteChildNodes(id);
            }
            else
            {
                await _activity.Delete(id);
            }
            return Ok();
        }

        /// <summary>
        /// Updates activity if one with specified Id exists. Otherwise creates a new activity
        /// </summary>
        /// <response code="200">Newly created or updated activity</response>
        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(ActivityDTO))]
        public async Task<IHttpActionResult> Save(ActivityDTO curActionDTO)
        {
            var submittedActivityDO = Mapper.Map<ActivityDO>(curActionDTO);

            var resultActionDTO = await _activity.SaveOrUpdateActivity(submittedActivityDO);

            return Ok(resultActionDTO);
        }
    }
}