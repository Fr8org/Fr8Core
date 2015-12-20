using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
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
using Utilities.Interfaces;

namespace HubWeb.Controllers
{
    //[RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class RoutesController : ApiController
    {
	    private const string PUSHER_EVENT_GENERIC_SUCCESS = "fr8pusher_generic_success";
	    private const string PUSHER_EVENT_GENERIC_FAILURE = "fr8pusher_generic_failure";

        private readonly IRoute _route;
        private readonly IFindObjectsRoute _findObjectsRoute;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
	    private readonly IPusherNotifier _pusherNotifier;
        
        public RoutesController()
        {
			_route = ObjectFactory.GetInstance<IRoute>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _findObjectsRoute = ObjectFactory.GetInstance<IFindObjectsRoute>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
	        _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
        }

        [Fr8ApiAuthorize]
        //[Route("full/{id:guid}")]
        [ActionName("full")]
        [ResponseType(typeof(RouteFullDTO))]
        [HttpGet]
        public IHttpActionResult GetFullRoute(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = uow.RouteRepository.GetByKey(id);
                var result = RouteMappingHelper.MapRouteToDto(uow, route);

                return Ok(result);
            };
        }

        //[Route("getByAction/{id:guid}")]
        [ResponseType(typeof(RouteFullDTO))]
        [HttpGet]
        
        public IHttpActionResult GetByAction(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var action = uow.ActionRepository.GetByKey(id);
                var route = _route.GetRoute(action);
                var result = RouteMappingHelper.MapRouteToDto(uow, route);

                return Ok(result);
            };
        }

        [Fr8ApiAuthorize]
        [ActionName("status")]
        [HttpGet]
        public IHttpActionResult GetByStatus(Guid? id = null, int? status = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRoutes = _route.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    _security.IsCurrentUserHasRole(Roles.Admin),
                    id,
                    status
                );

                if (curRoutes.Any())
                {
                    var queryableRepoOrdered = curRoutes.OrderByDescending(x => x.LastUpdated);
                    return Ok(queryableRepoOrdered.Select(Mapper.Map<RouteEmptyDTO>).ToArray());
                }
            }

            return Ok();
       }

        [Fr8ApiAuthorize]
        //[Route("copy")]
        [HttpPost]
        public IHttpActionResult Copy(Guid id, string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRouteDO = uow.RouteRepository.GetByKey(id);
                if (curRouteDO == null)
                {
                    throw new ApplicationException("Unable to find route with specified id.");
                }

                var route = _route.Copy(uow, curRouteDO, name);
                uow.SaveChanges();

                return Ok(new { id = route.Id });
            }
        }
        
        // GET api/<controller>
        [Fr8ApiAuthorize]
        public IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRoutes = _route.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    _security.IsCurrentUserHasRole(Roles.Admin),
                    id
                );

            if (curRoutes.Any())
            {
                // Return first record from curRoutes, in case id parameter was provided.
                // User intentionally wants to receive a single JSON object in response.
                if (id.HasValue)
                {
                    return Ok(Mapper.Map<RouteEmptyDTO>(curRoutes.First()));
                }

                // Return JSON array of objects, in case no id parameter was provided.
                return Ok(curRoutes.Select(Mapper.Map<RouteEmptyDTO>).ToArray());
            }
            }

            //DO-840 Return empty view as having empty process templates are valid use case.
            return Ok();
        }

        //[Route("~/routes")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Post(RouteEmptyDTO routeDto, bool updateRegistrations = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(routeDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }

                var curRouteDO = Mapper.Map<RouteEmptyDTO, RouteDO>(routeDto, opts => opts.Items.Add("ptid", routeDto.Id));
                curRouteDO.Fr8Account = _security.GetCurrentAccount(uow);

                //this will return 0 on create operation because of not saved changes
                _route.CreateOrUpdate(uow, curRouteDO, updateRegistrations);
                uow.SaveChanges();
                routeDto.Id = curRouteDO.Id;
                //what a mess lets try this
                /*curRouteDO.StartingSubroute.Route = curRouteDO;
                uow.SaveChanges();
                processTemplateDto.Id = curRouteDO.Id;*/
                return Ok(routeDto);
            }
        }

        
        [HttpPost]
        [ActionName("action")]
        [Fr8ApiAuthorize]
        public IHttpActionResult PutAction(ActionDTO actionDto)
        {
            //A stub until the functionaltiy is ready
            return Ok();
        }

        

        [HttpDelete]
        //[Route("{id:guid}")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Delete(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _route.Delete(uow, id);

                uow.SaveChanges();
                return Ok(id);
            }
        }

        
        [ActionName("triggersettings"), ResponseType(typeof(List<ExternalEventDTO>))]
        [Fr8ApiAuthorize]
        public IHttpActionResult GetTriggerSettings()
        {
            return Ok("This is no longer used due to V2 Event Handling mechanism changes.");
        }

        [HttpPost]
        //[Route("activate")]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Activate(RouteDO curRoute)
        {
            string actionDTO = await _route.Activate(curRoute);
            return Ok(actionDTO);
        }

        [HttpPost]
        //[Route("deactivate")]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Deactivate(RouteDO curRoute)
        {
            string actionDTO = await _route.Deactivate(curRoute);
            return Ok(actionDTO);
        }

        [HttpPost]
        //[Route("find_objects/create")]
        [Fr8ApiAuthorize]
        public IHttpActionResult CreateFindObjectsRoute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = uow.UserRepository.GetByKey(User.Identity.GetUserId());
                var route = _findObjectsRoute.CreateRoute(uow, account);

                uow.SaveChanges();

                return Ok(new { id = route.Id });
            }
        }

        [Fr8ApiAuthorize]
        //[Route("run")]
        [HttpPost]
        public async Task<IHttpActionResult> Run(Guid routeId, [FromBody]PayloadVM model)
        {
			CrateDTO curCrateDto;
            Crate curCrate = null;

			string pusherChannel = String.Format("fr8pusher_{0}", User.Identity.Name);

			if (model != null)
			{
				try
				{
                    curCrateDto = JsonConvert.DeserializeObject<CrateDTO>(model.Payload);
                    curCrate = _crate.FromDto(curCrateDto);
                }
                catch (Exception ex)
        {
					_pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, "You payload is invalid. Make sure that it represents a valid crate object JSON.");

					return BadRequest();
				}
			}

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var routeDO = uow.RouteRepository.GetByKey(routeId);

                try
                {
                    var containerDO = await _route.Run(routeDO, curCrate);

	                string message = String.Format("Route \"{0}\" executed", routeDO.Name);

					_pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_SUCCESS, message);

                    return Ok(Mapper.Map<ContainerDTO>(containerDO));
                }
                catch
                {
	                string message = String.Format("Route \"{0}\" failed", routeDO.Name);

					_pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, message);
                }

                return Ok();
            }
        }
    }
}