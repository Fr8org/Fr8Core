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

namespace HubWeb.Controllers
{
    //[RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class RoutesController : ApiController
    {
        private const string PUSHER_EVENT_GENERIC_SUCCESS = "fr8pusher_generic_success";
        private const string PUSHER_EVENT_GENERIC_FAILURE = "fr8pusher_generic_failure";

        private readonly Hub.Interfaces.IPlan _plan;
        private readonly IFindObjectsRoute _findObjectsRoute;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IPusherNotifier _pusherNotifier;

        public RoutesController()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _findObjectsRoute = ObjectFactory.GetInstance<IFindObjectsRoute>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
        }
        /*
        //[Route("~/routes")]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
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

                var curPlanDO = Mapper.Map<RouteEmptyDTO, RouteDO>(routeDto, opts => opts.Items.Add("ptid", routeDto.Id));
                curPlanDO.Fr8Account = _security.GetCurrentAccount(uow);

                //this will return 0 on create operation because of not saved changes
                _plan.CreateOrUpdate(uow, curPlanDO, updateRegistrations);
                uow.SaveChanges();
                routeDto.Id = curPlanDO.Id;
                //what a mess lets try this
                /*curPlanDO.StartingSubroute.Route = curPlanDO;
                uow.SaveChanges();
                processTemplateDto.Id = curPlanDO.Id;
                return Ok(routeDto);
            }
        }
        */
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(RouteFullDTO))]
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
                var curPlanDO = Mapper.Map<RouteEmptyDTO, PlanDO>(routeDto, opts => opts.Items.Add("ptid", routeDto.Id));

                _plan.CreateOrUpdate(uow, curPlanDO, updateRegistrations);

                uow.SaveChanges();
                var result = RouteMappingHelper.MapRouteToDto(uow, uow.PlanRepository.GetById<PlanDO>(curPlanDO.Id));
                return Ok(result);
            }
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
                var plan = uow.PlanRepository.GetById<PlanDO>(id);
                var result = RouteMappingHelper.MapRouteToDto(uow, plan);

                return Ok(result);
            };
        }

        //[Route("getByAction/{id:guid}")]
        [ResponseType(typeof(RouteFullDTO))]
        [HttpGet]

        public IHttpActionResult GetByActivity(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = _plan.GetPlanByActivityId(uow, id);
                var result = RouteMappingHelper.MapRouteToDto(uow, plan);

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
                var curPlans = _plan.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    _security.IsCurrentUserHasRole(Roles.Admin),
                    id,
                    status
                );

                if (curPlans.Any())
                {
                    var queryableRepoOrdered = curPlans.OrderByDescending(x => x.LastUpdated);
                    return Ok(queryableRepoOrdered.Select(Mapper.Map<RouteEmptyDTO>).ToArray());
                }
            }

            return Ok();
        }

        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<RouteFullDTO>))]
        public IHttpActionResult GetByName(string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlans = _plan.GetByName(uow, _security.GetCurrentAccount(uow), name);
                var fullRoutes = curPlans.Select(curPlan => RouteMappingHelper.MapRouteToDto(uow, curPlan)).ToList();
                return Ok(fullRoutes);

            }

        }

        [Fr8ApiAuthorize]
        //[Route("copy")]
        [HttpPost]
        public IHttpActionResult Copy(Guid id, string name)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = uow.PlanRepository.GetById<PlanDO>(id);
                if (curPlanDO == null)
                {
                    throw new ApplicationException("Unable to find plan with specified id.");
                }

                var plan = _plan.Copy(uow, curPlanDO, name);
                uow.SaveChanges();

                return Ok(new { id = plan.Id });
            }
        }

        // GET api/<controller>
        [Fr8ApiAuthorize]
        public IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlans = _plan.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    _security.IsCurrentUserHasRole(Roles.Admin),
                    id
                );

                if (curPlans.Any())
                {
                    // Return first record from curPlans, in case id parameter was provided.
                    // User intentionally wants to receive a single JSON object in response.
                    if (id.HasValue)
                    {
                        return Ok(Mapper.Map<RouteEmptyDTO>(curPlans.First()));
                    }

                    // Return JSON array of objects, in case no id parameter was provided.
                    return Ok(curPlans.Select(Mapper.Map<RouteEmptyDTO>).ToArray());
                }
            }

            //DO-840 Return empty view as having empty process templates are valid use case.
            return Ok();
        }

        [HttpPost]
        [ActionName("activity")]
        [Fr8ApiAuthorize]
        public IHttpActionResult PutActivity(ActivityDTO activityDto)
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
                _plan.Delete(uow, id);

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
        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Activate(Guid planId, bool routeBuilderActivate = false)
        {
            string pusherChannel = String.Format("fr8pusher_{0}", User.Identity.Name);

            try
            {
                var activateDTO = await _plan.Activate(planId, routeBuilderActivate);

                //check if the response contains any error message and show it to the user 
                if (activateDTO != null && activateDTO.ErrorMessage != string.Empty)
                    _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, activateDTO.ErrorMessage);

                return Ok(activateDTO);
            }
            catch (ApplicationException ex)
            {
                _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, ex.Message);
                return BadRequest();
            }
            catch (Exception)
            {
                _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, "There is a problem with activating this plan. Please try again later.");
                return BadRequest();
            }
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Deactivate(Guid planId)
        {
            var eventManager = ObjectFactory.GetInstance<Event>();
            string activityDTO = await _plan.Deactivate(planId);
            await eventManager.Publish("RouteDeactivated", ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(), planId.ToString(), null, "Success");

            return Ok(activityDTO);
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        public IHttpActionResult CreateFindObjectsRoute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = uow.UserRepository.GetByKey(User.Identity.GetUserId());
                var plan = _findObjectsRoute.CreatePlan(uow, account);

                uow.SaveChanges();

                return Ok(new { id = plan.Id });
            }
        }

        [Fr8ApiAuthorize("Admin", "Customer")]
        [HttpPost]
        public async Task<IHttpActionResult> Run(Guid planId, [FromBody]PayloadVM model)
        {
            //ACTIVATE - activate route if its inactive
            var eventManager = ObjectFactory.GetInstance<Event>();
            bool inActive = false;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var routeDO = uow.PlanRepository.GetById<PlanDO>(planId);

                if (routeDO.RouteState == RouteState.Inactive)
                    inActive = true;
            }

            string pusherChannel = String.Format("fr8pusher_{0}", User.Identity.Name);

            if (inActive)
            {
                var activateDTO = await _plan.Activate(planId, false);

                if (activateDTO != null && activateDTO.Status == "validation_error")
                {
                    //this container holds wrapped inside the ErrorDTO
                    return Ok(activateDTO.Container);
                }

                await eventManager.Publish("RouteActivated", ObjectFactory.GetInstance<ISecurityServices>().GetCurrentAccount
                    (ObjectFactory.GetInstance<IUnitOfWork>()).Id.ToString(), planId.ToString(), JsonConvert.SerializeObject(planId).ToString(), "Success");
            }

            //RUN
            CrateDTO curCrateDto;
            Crate curCrate = null;

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
                var planDO = uow.PlanRepository.GetById<PlanDO>(planId);

                try
                {
                    if (planDO != null)
                    {
                        _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_SUCCESS,
                            string.Format("Launching a new Container for Plan \"{0}\"", planDO.Name));

                        var containerDO = await _plan.Run(planDO, curCrate);

                        var response = _crate.GetContentType<OperationalStateCM>(containerDO.CrateStorage);

                        string responseMsg = "";

                        ResponseMessageDTO responseMessage;
                        if (response != null && response.CurrentActivityResponse != null && response.CurrentActivityResponse.TryParseResponseMessageDTO(out responseMessage))
                        {
                            if (responseMessage != null && !string.IsNullOrEmpty(responseMessage.Message))
                            {
                                responseMsg = "\n" + responseMessage.Message;
                            }
                        }

                        string message = String.Format("Complete processing for Plan \"{0}\".{1}", planDO.Name, responseMsg);

                        _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_SUCCESS, message);

                        var containerDTO = Mapper.Map<ContainerDTO>(containerDO);

                        await eventManager.Publish("ContainerLaunched"
                            , planDO.Fr8AccountId
                            , planDO.Id.ToString()
                            , JsonConvert.SerializeObject(containerDTO).ToString(), "Success");

                        await eventManager.Publish("ContainerExecutionComplete"
                            , planDO.Fr8AccountId
                            , planDO.Id.ToString()
                            , JsonConvert.SerializeObject(containerDTO).ToString(), "Success");

                        return Ok(containerDTO);
                    }

                    return BadRequest();
                }
                catch (ErrorResponseException exception)
                {
                    //this response contains details about the error that happened on some terminal and need to be shown to client
                    return Ok(exception.ContainerDTO);
                }
                catch (Exception e)
                {
                    string message = String.Format("Plan \"{0}\" failed", planDO.Name);

                    _pusherNotifier.Notify(pusherChannel, PUSHER_EVENT_GENERIC_FAILURE, message);
                }

                return Ok();
            }
        }
    }
}