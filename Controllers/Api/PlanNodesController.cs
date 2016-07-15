using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class PlanNodesController : ApiController
    {
        private readonly IPlanNode _activity;

        public PlanNodesController()
        {
            _activity = ObjectFactory.GetInstance<IPlanNode>();
        }
        /// <summary>
        /// Retrieves collection of activities that are specifically positioned related to activity with specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of activity to use as a start point</param>
        /// <param name="direction">Direction of lookup. Allows only values of 'upsteam' and 'downstream' to search for preceeding and following activities respectively</param>
        [SwaggerResponse(HttpStatusCode.OK, "Collection of activities preceeding or following the specified one. Can be empty", typeof(List<ActivityDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Activity doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [Fr8TerminalAuthentication]
        public IHttpActionResult Get(Guid id, string direction)
        {
            direction = (direction ?? string.Empty).ToLower();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityDO activityDO = uow.PlanRepository.GetById<ActivityDO>(id);
                if (activityDO == null)
                {
                    throw new MissingObjectException($"Activity with Id {id} doesn't exist");
                }
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
        [HttpGet]
        [ActionName("signals")]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Object containing information about signalled crates and their fields", typeof(List<ActivityDTO>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Activity doesn't exist", typeof(DetailedMessageDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public IHttpActionResult GetAvailableData(Guid id, CrateDirection direction = CrateDirection.Upstream, AvailabilityType availability = AvailabilityType.RunTime)
        {
            return Ok(_activity.GetIncomingData(id, direction, availability));
        }
    }
}