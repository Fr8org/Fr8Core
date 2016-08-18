using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Helper;
using Newtonsoft.Json;
using Hub.Infrastructure;
using HubWeb.Infrastructure_HubWeb;
using HubWeb.ViewModels.RequestParameters;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    //[RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class PlansController : ApiController
    {

        private readonly IPlan _plan;
        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly IPlanDirectoryService _planDirectoryService;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IPusherNotifier _pusherNotifier;

        public PlansController( IPlan plan, 
                                ISecurityServices securityServices, 
                                ICrateManager crateManager, 
                                IPusherNotifier pusherNotifier, 
                                IActivityTemplate activityTemplate, 
                                IActivity activity, 
                                IPlanDirectoryService planDirectoryService)
        {
            _plan = plan;
            _security = securityServices;
            _crate = crateManager;
            _pusherNotifier = pusherNotifier;
            _activityTemplate = activityTemplate;
            _activity = activity;
            _planDirectoryService = planDirectoryService;
        }

        /// <summary>
        /// Creates or updates plan. If parameters contain name of solution, creates plan with solution with given name
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="planDto">Description of plan to create or update</param>
        /// <param name="solutionName">Name of solution to create if specified</param>
        /// <param name="updateRegistrations">Deprecated</param>
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Created or updated plan", typeof(PlanDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request")]
        public async Task<IHttpActionResult> Post([FromBody] PlanNoChildrenDTO planDto, [FromUri] PlansPostParams parameters = null)
        {
            parameters = parameters ?? new PlansPostParams();

            if (!parameters.solutionName.IsNullOrEmpty())
            {
                return await CreateSolution(parameters.solutionName);
            }

            return await Post(planDto);
        }

        [HttpPost]
        [NonAction]
        private async Task<IHttpActionResult> CreateSolution(string solutionName)
        {
            var userId = User.Identity.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActivityTemplateDO activityTemplate;

                var activityTemplateInfo = _activityTemplate.GetActivityTemplateInfo(solutionName);

                if (!string.IsNullOrEmpty(activityTemplateInfo.Version))
                {
                    activityTemplate = _activityTemplate
                        .GetQuery()
                        .FirstOrDefault(x => x.Name == activityTemplateInfo.Name && x.Version == activityTemplateInfo.Version);
                }
                else
                {
                    activityTemplate = _activityTemplate.GetQuery()
                        .Where(x => x.Name == solutionName)
                        .AsEnumerable()
                        .OrderByDescending(x => int.Parse(x.Version))
                        .FirstOrDefault();
                }
                if (activityTemplate == null)
                {
                    throw new MissingObjectException($"Activity template (solution) name {solutionName} doesn't exist");
                }
                ObjectFactory.GetInstance<ITracker>().Track(_security.GetCurrentAccount(uow), "Loaded Solution", new Segment.Model.Properties().Add("Solution Name", solutionName));
                var result = await _activity.CreateAndConfigure(
                    uow,
                    userId,
                    activityTemplate.Id,
                    name: activityTemplate.Label,
                    createPlan: true);
                return Ok(PlanMappingHelper.MapPlanToDto((PlanDO)result));
            }
        }

        [Fr8TerminalAuthentication]
        [ResponseType(typeof(PlanDTO))]
        [NonAction]
        private async Task<IHttpActionResult> Post(PlanNoChildrenDTO planDto)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Some of the request data is invalid");
                }
                var curPlanDO = Mapper.Map<PlanNoChildrenDTO, PlanDO>(planDto, opts => opts.Items.Add("ptid", planDto.Id));

                _plan.CreateOrUpdate(uow, curPlanDO);

                uow.SaveChanges();
                var result = PlanMappingHelper.MapPlanToDto(_plan.GetFullPlan(uow, curPlanDO.Id));
                return Ok(result);
            }
        }
        /// <summary>
        /// Retrieves collections of plans filtered by specified parameters
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="planQuery">Query parameters</param>
        /// <response code="200">Filtered collection of plans</response>
        /// <response code="403">Unauthorized request</response>
        [Fr8ApiAuthorize]
        [HttpGet]
        [ResponseType(typeof(PlanResultDTO))]
        public IHttpActionResult Query([FromUri] PlanQueryDTO planQuery)
        {
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

        /// <summary>
        /// Retrieves specific plans based on query parameters
        /// </summary>
        /// <remarks>
        /// If id is defined - retrieves plan by Id, also retrieves plan with child nodes if include_children is defined. <br/>
        /// If activity_id is defined - retrieves plan that contains activity with specified Id. <br/>
        /// If name is defined - retrieves plan by its name. <br />
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <param name="parameters">Query parameters</param>
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "Plan satysfying query parameters", typeof(PlanDTO))]
        [SwaggerResponse(HttpStatusCode.OK, "Collection of plans queried by name", typeof(List<PlanDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Multiple query parameters are defined")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> Get([FromUri] PlansGetParams parameters)
        {
            if ((!parameters.name.IsNullOrEmpty() && parameters.id.HasValue)
                || (parameters.activity_id.HasValue && parameters.id.HasValue)
                || (parameters.activity_id.HasValue && !parameters.name.IsNullOrEmpty()))
            {
                return BadRequest("Multiple parameters are defined");
            }
            if (parameters.include_children && parameters.id.HasValue)
            {
                return await GetFullPlan((Guid)parameters.id);
            }
            if (parameters.activity_id.HasValue)
            {
                return GetByActivity((Guid)parameters.activity_id);
            }
            if (!parameters.name.IsNullOrEmpty())
            {
                return GetByName(parameters.name, parameters.visibility);
            }
            return Get(parameters.id);
        }

        [ResponseType(typeof(PlanDTO))]
        [HttpGet]
        [NonAction]
        private async Task<IHttpActionResult> GetFullPlan(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = _plan.GetFullPlan(uow, id);
                var result = PlanMappingHelper.MapPlanToDto(plan);

                result.Visibility.Public = await _planDirectoryService.GetTemplate(id, User.Identity.GetUserId()) != null;

                return Ok(result);
            };
        }

        [Fr8TerminalAuthentication]
        [ResponseType(typeof(PlanDTO))]
        [NonAction]
        private IHttpActionResult GetByActivity(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = _plan.GetPlanByActivityId(uow, id);
                var result = PlanMappingHelper.MapPlanToDto(plan);

                return Ok(result);
            };
        }

        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [ResponseType(typeof(List<PlanDTO>))]
        [NonAction]
        private IHttpActionResult GetByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlans = _plan.GetByName(uow, _security.GetCurrentAccount(uow), name, visibility);
                var fullPlans = curPlans.Select(curPlan => PlanMappingHelper.MapPlanToDto(curPlan)).ToList();
                return Ok(fullPlans);

            }

        }

        [NonAction]
        private IHttpActionResult Get(Guid? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planResult = _plan.GetForUser(
                    uow,
                    _security.GetCurrentAccount(uow),
                    new PlanQueryDTO() { Id = id },
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
        /// <summary>
        /// Deletes plan with specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of plan to delete</param>
        /// <response code="200">Id of deleted plan</response>
        /// <response code="403">Unauthorized request</response>
        [HttpDelete]
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Id of deleted plan")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Plan doesn't exist", typeof(DetailedMessageDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request")]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            await _plan.Delete(id);

            return Ok(id);
        }
        /// <summary>
        /// Deactivates monitoring plan with specified Id
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="planId">Id of plan to deactivate</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Plan was successfully deactivated")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Plan doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Deactivate(Guid planId)
        {
            await _plan.Deactivate(planId);
            return Ok();
        }
        /// <summary>
        /// Uploads file with plan template and create plan from it. 
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <param name="planName">Name to assign to plan that will be created</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Uploaded plan", typeof(PlanNoChildrenDTO))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Bad format of plan file", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> Upload(string planName)
        {
            IHttpActionResult result = InternalServerError();
            await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider()).ContinueWith( async (tsk) =>
            {
                MultipartMemoryStreamProvider prvdr = tsk.Result;

                foreach (HttpContent ctnt in prvdr.Contents)
                {
                    Stream stream = ctnt.ReadAsStreamAsync().Result;

                    var content = new StreamReader(stream).ReadToEnd();

                    var planTemplateDTO = JsonConvert.DeserializeObject<PlanDTO>(content);
                    planTemplateDTO.Name = planName;

                    result = await Load(planTemplateDTO);
                }
            });

            return result;
        }

        /// <summary>
        /// Executes plan with specified Id passing specific payload to it
        /// </summary>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <param name="planId">Id of plan to execute</param>
        /// <param name="payload">Payload to provide to plan during execution</param>
        [Fr8ApiAuthorize("Admin", "StandardUser", "Terminal", "Guest")]
        [Fr8TerminalAuthentication]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Container creating during successful plan execution", typeof(ContainerDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Plan with specified Id doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> Run(Guid planId, [FromBody] CrateDTO[] payload)
        {
            Crate[] crates = null;
            if (payload != null)
            {
                crates = payload.Select(c => _crate.FromDto(c)).ToArray();
            }
            var result = await _plan.Run(planId, crates, null);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        /// <summary>
        /// Builds plan template based on the plan with specified Id
        /// </summary>
        /// <param name="planId">Id of plan to build template from</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        [Fr8ApiAuthorize("Admin", "StandardUser", "Terminal")]
        [Fr8TerminalAuthentication]
        [ResponseType(typeof(PlanDTO))]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Plan template was built successfully", typeof(ContainerDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Plan with specified Id doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> Templates(Guid planId)
        {
            var planTemplateDTO = _planDirectoryService.CrateTemplate(planId, User.Identity.GetUserId());
            return Ok(planTemplateDTO);
        }
        /// <summary>
        /// Shares plan with specified Id in plan directory so it will be available to other users
        /// </summary>
        /// <param name="planId">Id of plan to share</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        [Fr8ApiAuthorize("Admin", "StandardUser", "Terminal")]
        [Fr8TerminalAuthentication]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Plan was successfully shared")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Plan with specified Id doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Share(Guid planId)
        {
            await _planDirectoryService.Share(planId, User.Identity.GetUserId());
            return Ok();
        }
        /// <summary>
        /// Removes plan with specified Id from the plan directory so it will be no longer available to other users
        /// </summary>
        /// <param name="planId">Id of plan to remove from plan directory</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        [Fr8ApiAuthorize("Admin", "StandardUser", "Terminal")]
        [Fr8TerminalAuthentication]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Plan was successfully removed from plan directory")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Plan with specified Id doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> Unpublish(Guid planId)
        {
            var identity = User.Identity as ClaimsIdentity;
            var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");

            await _planDirectoryService.Unpublish(planId, User.Identity.GetUserId(), privileged);
            return Ok();
        }
        /// <summary>
        /// Builds a plan from specified plan template
        /// </summary>
        /// <param name="plan">Plan template to build a plan from</param>
        /// <remarks>
        /// Fr8 authentication headers must be provided
        /// </remarks>
        /// <response code="200">Plan was successfully built from template</response>
        /// <response code="403">Unauthorized request</response>
        [Fr8ApiAuthorize("Admin", "StandardUser", "Terminal")]
        [Fr8TerminalAuthentication]
        [Fr8PlanDirectoryAuthentication]
        [HttpPost]
        [ResponseType(typeof(PlanNoChildrenDTO))]
        public async Task<IHttpActionResult> Load(PlanDTO plan)
        {
            return Ok(await _planDirectoryService.CreateFromTemplate(plan, User.Identity.GetUserId()));
        }
    }
}