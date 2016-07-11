using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    public class PlanNodesController : ApiController
    {
        private readonly IPlanNode _activity;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IActivityTemplate _activityTemplate;

        public PlanNodesController()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
        }
        /// <summary>
        /// Retrieves collection of activity that are specifically positioned related to activity with specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of activity to use as a start point</param>
        /// <param name="direction">Direction of lookup. Allows only values of 'upsteam' and 'downstream' to search for preceeding and following activities respectively</param>
        /// <response code="200">Collection of activities preceeding or following the specified one. Can be empty</response>
        /// <response code="403">Unauthorized request</response>
        [ResponseType(typeof(List<ActivityDTO>))]
        [Fr8TerminalAuthentication]
        public IHttpActionResult Get(Guid id, string direction)
        {
            direction = (direction ?? string.Empty).ToLower();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityDO activityDO = uow.PlanRepository.GetById<ActivityDO>(id);
                switch(direction)
                {
                    case "upstream":
                        return Ok(_activity.GetUpstreamActivities(uow, activityDO).
                            OfType<ActivityDO>().Select(x => Mapper.Map<ActivityDTO>(x)).ToList());
                    case "downstream":
                        return Ok(_activity.GetDownstreamActivities(uow, activityDO).
                            OfType<ActivityDO>().Select(x => Mapper.Map<ActivityDTO>(x)).ToList());
                    default:
                        return Ok();
                }
            }
        }
        /// <summary>
        /// Retrieves the list of crates and fields signalled by activities positioned related to activity with specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of activity to use as a start point</param>
        /// <param name="direction">Direction of lookup. 0 for preceeding activities, 1 for following activities, 2 for both</param>
        /// <param name="availability">Bitwise combination of crates and fields availability types. 0 - not set, 1 - available at plan configuration time, 2 - available at plan execution time</param>
        /// <response code="200">Object containing information about signalled crates and their fields</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [ActionName("signals")]
        [Fr8TerminalAuthentication]
        [ResponseType(typeof(IncomingCratesDTO))]
        public IHttpActionResult GetAvailableData(Guid id, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)
        {
            return Ok(_activity.GetIncomingData(id, direction, availability));
        }
    }
}