using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Exceptions;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    [RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class RouteController : ApiController
    {
        private readonly IRoute _route;
        private readonly IFindObjectsRoute _findObjectsRoute;
        private readonly ISecurityServices _security;
        
        public RouteController()
            : this(ObjectFactory.GetInstance<IRoute>())
        {
        }

        

        public RouteController(IRoute route)
        {
            _route = route;
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _findObjectsRoute = ObjectFactory.GetInstance<IFindObjectsRoute>();
        }

        [Fr8ApiAuthorize]
        [Route("full/{id:int}")]
        [ResponseType(typeof(RouteFullDTO))]
        [HttpGet]
        public IHttpActionResult GetFullRoute(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = uow.RouteRepository.GetByKey(id);
                var result = _route.MapRouteToDto(uow, route);

                return Ok(result);
            };
        }

        [Route("getByAction/{id:int}")]
        [ResponseType(typeof(RouteFullDTO))]
        [HttpGet]
        
        public IHttpActionResult GetByAction(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var action = uow.ActionRepository.GetByKey(id);
                var route = _route.GetRoute(action);
                var result = _route.MapRouteToDto(uow, route);

                return Ok(result);
            };
        }

        [Fr8ApiAuthorize]
        [Route("status")]
        [HttpGet]
        public IHttpActionResult GetByStatus(int? id = null, int? status = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRoutes = _route.GetForUser(uow, _security.GetCurrentAccount(uow), _security.IsCurrentUserHasRole(Roles.Admin), id, status);

                if (curRoutes.Any())
                {               
                    return Ok(curRoutes.Select(Mapper.Map<RouteEmptyDTO>).ToArray());
                }
            }

            return Ok();
       }

        [Fr8ApiAuthorize]
        [Route("copy")]
        [HttpPost]
        public IHttpActionResult Copy(int id, string name)
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
        public IHttpActionResult Get(int? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRoutes = _route.GetForUser(uow, _security.GetCurrentAccount(uow), _security.IsCurrentUserHasRole(Roles.Admin), id);

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

        [Route("~/routes")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Post(RouteEmptyDTO processTemplateDto, bool updateRegistrations = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(processTemplateDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }

                var curRouteDO = Mapper.Map<RouteEmptyDTO, RouteDO>(processTemplateDto, opts => opts.Items.Add("ptid", processTemplateDto.Id));
                curRouteDO.Fr8Account = _security.GetCurrentAccount(uow);

                //this will return 0 on create operation because of not saved changes
                _route.CreateOrUpdate(uow, curRouteDO, updateRegistrations);
                uow.SaveChanges();
                processTemplateDto.Id = curRouteDO.Id;
                //what a mess lets try this
                /*curRouteDO.StartingSubroute.Route = curRouteDO;
                uow.SaveChanges();
                processTemplateDto.Id = curRouteDO.Id;*/
                return Ok(processTemplateDto);
            }
        }

        
        [HttpPost]
        [Route("action")]
        [ActionName("action")]
        [Fr8ApiAuthorize]
        public IHttpActionResult PutAction(ActionDTO actionDto)
        {
            //A stub until the functionaltiy is ready
            return Ok();
        }

        

        [HttpDelete]
        [Route("{id:int}")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                _route.Delete(uow, id);

                uow.SaveChanges();
                return Ok(id);
            }
        }

        
        [Route("triggersettings"), ResponseType(typeof(List<ExternalEventDTO>))]
        [Fr8ApiAuthorize]
        public IHttpActionResult GetTriggerSettings()
        {
            return Ok("This is no longer used due to V2 Event Handling mechanism changes.");
        }

        [HttpPost]
        [Route("activate")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Activate(RouteDO curRoute)
        {
            return Ok(_route.Activate(curRoute));
        }

        [HttpPost]
        [Route("deactivate")]
        [Fr8ApiAuthorize]
        public IHttpActionResult Deactivate(RouteDO curRoute)
        {
            return Ok(_route.Deactivate(curRoute));
        }

        [HttpPost]
        [Route("find_objects/create")]
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
    }
}