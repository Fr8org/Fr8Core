using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Hub.Exceptions;
using HubWeb.Controllers.Helpers;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using System.Threading.Tasks;
using HubWeb.ViewModels;
using Newtonsoft.Json;
using Hub.Infrastructure;
using Hub.Managers;
using Utilities.Interfaces;
using Data.Infrastructure;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    //[RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class PlansController : ApiController
    {

        private readonly Hub.Interfaces.IPlan _plan;

        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly IFindObjectsPlan _findObjectsPlan;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IPusherNotifier _pusherNotifier;

        public PlansController()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _findObjectsPlan = ObjectFactory.GetInstance<IFindObjectsPlan>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
        }

        [HttpPost]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> Create(Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, Guid? authorizationTokenId = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userId = User.Identity.GetUserId();
                var result = await _activity.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, true, authorizationTokenId) as PlanDO;
                return Ok(Mapper.Map<PlanDTO>(result));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateSolution(string solutionName)
        {
            var userId = User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityTemplate = _activityTemplate.GetQuery().FirstOrDefault(at => at.Name == solutionName);
                if (activityTemplate == null)
                {
                    throw new ArgumentException($"actionTemplate (solution) name {solutionName} is not found in the database.");
                }
                var result = await _activity.CreateAndConfigure(
                    uow, 
                    userId, 
                    activityTemplate.Id, 
                    name: activityTemplate.Label, 
                    createPlan: true);
                return Ok(PlanMappingHelper.MapPlanToDto(uow, (PlanDO)result));
            }
        }

        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(PlanDTO))]
        public IHttpActionResult Post(PlanEmptyDTO planDto, bool updateRegistrations = false)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (string.IsNullOrEmpty(planDto.Name))
                {
                    ModelState.AddModelError("Name", "Name cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }
                var curPlanDO = Mapper.Map<PlanEmptyDTO, PlanDO>(planDto, opts => opts.Items.Add("ptid", planDto.Id));

                _plan.CreateOrUpdate(uow, curPlanDO);

                uow.SaveChanges();
                var result = PlanMappingHelper.MapPlanToDto(uow, uow.PlanRepository.GetById<PlanDO>(curPlanDO.Id));
                return Ok(result);
            }
        }

        [Fr8ApiAuthorize]
        //[Route("full/{id:guid}")]
        [ActionName("full")]
        [ResponseType(typeof(PlanDTO))]
        [HttpGet]
        public IHttpActionResult GetFullPlan(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(id);
                var result = PlanMappingHelper.MapPlanToDto(uow, plan);

                return Ok(result);
            };
        }

        //[Route("getByAction/{id:guid}")]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(PlanDTO))]
        [HttpGet]
        public IHttpActionResult GetByActivity(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = _plan.GetPlanByActivityId(uow, id);
                var result = PlanMappingHelper.MapPlanToDto(uow, plan);

                return Ok(result);
            };
        }

        [Fr8ApiAuthorize]
        [ActionName("getByQuery")]
        [HttpGet]
        public IHttpActionResult GetByQuery([FromUri] PlanQueryDTO planQuery)
        {
            //i want to leave md-data-tables related logic inside controller
            //that is why this operation is done here - our backend service shouldn't know anything
            //about frontend libraries
            if (planQuery != null && planQuery.OrderBy.StartsWith("-"))
            {
                planQuery.IsDescending = true;
            }
            else if (planQuery != null && !planQuery.OrderBy.StartsWith("-"))
            {
                planQuery.IsDescending = false;
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planResult = _plan.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    planQuery,
                    _security.IsCurrentUserHasRole(Roles.Admin)
                );

                return Ok(planResult);
            }
        }

        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<PlanDTO>))]
        public IHttpActionResult GetByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlans = _plan.GetByName(uow, _security.GetCurrentAccount(uow), name, visibility);
                var fullPlans = curPlans.Select(curPlan => PlanMappingHelper.MapPlanToDto(uow, curPlan)).ToList();
                return Ok(fullPlans);

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
        /// <summary>
        /// TODO this function shouldn't exist
        /// inspect it's usage and remove this from project
        /// Get should use PlanQueryDTO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Fr8ApiAuthorize]
        public IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planResult = _plan.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    new PlanQueryDTO() {Id = id}, 
                    _security.IsCurrentUserHasRole(Roles.Admin)
                );

                if (planResult.Plans.Any())
                {
                    // Return first record from curPlans, in case id parameter was provided.
                    // User intentionally wants to receive a single JSON object in response.
                    if (id.HasValue)
                    {
                        return Ok(planResult.Plans.First());
                    }

                    // Return JSON array of objects, in case no id parameter was provided.
                    return Ok(planResult.Plans);
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
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult Delete(Guid id)
        {
            _plan.Delete(id);

            return Ok(id);
        }


        [ActionName("triggersettings"), ResponseType(typeof(List<ExternalEventDTO>))]
        [Fr8ApiAuthorize]
        public IHttpActionResult GetTriggerSettings()
        {
            return Ok("This is no longer used due to V2 Event Handling mechanism changes.");
        }
        
        [HttpPost]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Deactivate(Guid planId)
        {
            await _plan.Deactivate(planId);
           
            return Ok();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        public IHttpActionResult CreateFindObjectsPlan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = uow.UserRepository.GetByKey(User.Identity.GetUserId());
                var plan = _findObjectsPlan.CreatePlan(uow, account);

                uow.SaveChanges();

                return Ok(new { id = plan.Id });
            }
        }

        // method for plan execution continuation from URL
        [Fr8ApiAuthorize("Admin", "Customer")]
        [HttpGet]
        public Task<IHttpActionResult> Run(Guid planId, Guid? containerId = null)
        {
            return Run(planId, (Crate[]) null, containerId);
        }

        [Fr8ApiAuthorize("Admin", "Customer")]
        [HttpPost]
        public async Task<IHttpActionResult> Run(Guid planId, [FromBody]PayloadVM model, Guid? containerId = null)
        {
            //RUN
            Crate[] curPayload = null;

            // there is no reason to check for payload if we have continerId passed because this indicates execution continuation scenario.
            if (model != null && containerId == null)
            {
                try
                {
                    var curCrateDto = JsonConvert.DeserializeObject<CrateDTO>(model.Payload);
                    curPayload = new[] { _crate.FromDto(curCrateDto) };
                }
                catch
                {
                    _pusherNotifier.NotifyUser("Your payload is invalid. Make sure that it represents a valid crate object JSON.",
                        NotificationChannel.GenericFailure,
                        User.Identity.Name);
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var planDO = uow.PlanRepository.GetById<PlanDO>(planId);
                        var currentPlanType = _plan.IsMonitoringPlan(uow, planDO) ? PlanType.Monitoring.ToString() : PlanType.RunOnce.ToString();
                        return BadRequest(currentPlanType);
                    }
                }
            }

            return await Run(planId, curPayload, containerId);
        }

        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public Task<IHttpActionResult> RunWithPayload(Guid planId, [FromBody]List<CrateDTO> payload)
        {
            var crates = payload.Select(c => _crate.FromDto(c)).ToArray();

            return Run(planId, crates, null);
        }

        // We don't have place in activity configuration pane to display activity-wide configuration errors that are not binded to specific controls.
        // Report them via Action Stream.
        private void ReportGenericValidationErrors(string activityLabel, string planName, ValidationErrorsDTO validationErrors)
        {
            var genericErrors = new List<string>();

            foreach (var error in validationErrors.ValidationErrors)
            {
                if (error.ControlNames == null || error.ControlNames.Count == 0)
                {
                    genericErrors.Add(error.ErrorMessage);
                }
            }

            if (genericErrors.Count > 0)
            {
                var errors = string.Join(" ", genericErrors.Select(x =>
                {
                    if (!x.TrimEnd().EndsWith("."))
                    {
                        return x + ".";
                    }

                    return x;
                }));

                _pusherNotifier.NotifyUser($"Validation of activity '{activityLabel}' from plan \"{planName}\" failed: {errors}",
                       NotificationChannel.GenericFailure,
                       User.Identity.Name);
            }
        }

        private async Task<IHttpActionResult> Run(Guid planId, Crate[] payload, Guid? containerId)
        {
            var activationResults = await _plan.Activate(planId, false);

            if (activationResults.ValidationErrors.Count > 0)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var plan = uow.PlanRepository.GetById<PlanDO>(planId);
                    var failedActivities = new List<string>();

                    foreach (var key in activationResults.ValidationErrors.Keys)
                    {
                        var activity = uow.PlanRepository.GetById<PlanNodeDO>(key) as ActivityDO;

                        if (activity != null)
                        {
                            var label = string.IsNullOrWhiteSpace(activity.Label) ? activity.Name : activity.Label;

                            if (string.IsNullOrWhiteSpace(label))
                            {
                                label = activity.Id.ToString();
                            }

                            failedActivities.Add($"'{label}'");

                            ReportGenericValidationErrors(label, plan.Name, activationResults.ValidationErrors[key]);
                        }
                    }

                    var activitiesList = string.Join(", ", failedActivities);

                    _pusherNotifier.NotifyUser($"Validation failed for activities: {activitiesList} from plan \"{plan.Name}\". See activity configuration pane for details.",
                        NotificationChannel.GenericFailure,
                        User.Identity.Name);
                }

                return Ok(new ContainerDTO
                {
                    PlanId = planId,
                    ValidationErrors = activationResults.ValidationErrors
                });
            }
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ContainerDO container;
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                // it container id was passed validate it
                if (containerId != null)
                {
                    container = uow.ContainerRepository.GetByKey(containerId.Value);

                    // we didn't find contaier or found container belong to the other plan.
                    if (container == null || container.PlanId != planId)
                    {
                        // that's bad. Reset containerId to run plan with new container
                        containerId = null;
                    }
                }

                PlanType? currentPlanType = null;

                try
                {
                    if (plan != null)
                    {
                        currentPlanType = _plan.IsMonitoringPlan(uow, plan) ? PlanType.Monitoring : PlanType.RunOnce;

                        if (containerId == null)
                        {
                            // There is no sense to run monitoring plans explicitly
                            // Just return empty container
                            if (currentPlanType == PlanType.Monitoring)
                            {
                                _pusherNotifier.NotifyUser($"Plan \"{plan.Name}\" activated. It will wait and respond to specified external events.",
                                       NotificationChannel.GenericSuccess,
                                       User.Identity.Name);

                                return Ok(new ContainerDTO
                                {
                                    CurrentPlanType = currentPlanType,
                                    PlanId = planId
                                });
                            }

                            container = await _plan.Run(plan.Id, payload);
                            _pusherNotifier.NotifyUser($"Launching a new Container for Plan \"{plan.Name}\"",
                                NotificationChannel.GenericSuccess,
                                User.Identity.Name);
                        }
                        else
                        {
                            container = await _plan.Continue(containerId.Value);
                            _pusherNotifier.NotifyUser($"Continue execution of the supsended Plan \"{plan.Name}\"",
                                NotificationChannel.GenericSuccess,
                                User.Identity.Name);
                        }

                        var response = _crate.GetContentType<OperationalStateCM>(container.CrateStorage);
                        var responseMsg = GetResponseMessage(response);

                        string message = String.Format("Complete processing for Plan \"{0}\".{1}", plan.Name, responseMsg);

                        _pusherNotifier.NotifyUser(message, NotificationChannel.GenericSuccess, User.Identity.Name);
                        EventManager.ContainerLaunched(container);

                        var containerDTO = Mapper.Map<ContainerDTO>(container);
                        containerDTO.CurrentPlanType = currentPlanType;

                        // THIS CODE IS HERE ONLY TO SUPPORT CURRENT UI LOGIC THAT DISPLAYS PLAN LISTS.
                        // It should be updated to show  as 'running' only:
                        //   1. Plans that have at least one executing container
                        //   2. Active monitoring plans
                        if (currentPlanType == PlanType.RunOnce)
                        {
                            using (var planStatUpdateUow = ObjectFactory.GetInstance<IUnitOfWork>())
                            {
                                planStatUpdateUow.PlanRepository.GetById<PlanDO>(planId).PlanState = PlanState.Inactive;
                                planStatUpdateUow.SaveChanges();
                            }
                        }

                        EventManager.ContainerExecutionCompleted(container);

                        return Ok(containerDTO);
                    }
                    
                    return BadRequest();
                }
                catch (InvalidTokenRuntimeException exception)
                {
                    //this response contains details about the error that happened on some terminal and need to be shown to client
                    if (exception.ContainerDTO != null)
                    {
                        exception.ContainerDTO.CurrentPlanType = currentPlanType;
                    }
                    // Do not notify -- it happens in Plan.cs
                    throw;
                }
                catch (ActivityExecutionException exception)
                {
                    //this response contains details about the error that happened on some terminal and need to be shown to client
                    if (exception.ContainerDTO != null)
                    {
                        exception.ContainerDTO.CurrentPlanType = currentPlanType;
                    }

                    NotifyWithErrorMessage(exception, plan, User.Identity.Name, exception.ErrorMessage);

                    throw;
                }
                catch (Exception e)
                {
                    var errorMessage = "An internal error has occured. Please, contact the administrator.";
                    NotifyWithErrorMessage(e, plan, User.Identity.Name, errorMessage);
                    throw;
                }
            }
        }

        private string GetResponseMessage(OperationalStateCM response)
        {
            string responseMsg = "";
            ResponseMessageDTO responseMessage;
            if (response != null && response.CurrentActivityResponse != null && response.CurrentActivityResponse.TryParseResponseMessageDTO(out responseMessage))
            {
                if (responseMessage != null && !string.IsNullOrEmpty(responseMessage.Message))
                {
                    responseMsg = "\n" + responseMessage.Message;
                }
            }

            return responseMsg;
        }

        private void NotifyWithErrorMessage(Exception exception, PlanDO planDO, string username, string errorMessage = "")
        {
            var messageToNotify = string.Empty;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                messageToNotify = errorMessage;
            }

            var message = String.Format("Plan \"{0}\" failed. {1}", planDO.Name, messageToNotify);
            _pusherNotifier.NotifyUser(message, NotificationChannel.GenericFailure, username);

        }
    }
}