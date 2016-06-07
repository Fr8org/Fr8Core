using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;
using fr8.Infrastructure.Data.Managers;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Interfaces;
using fr8.Infrastructure.Utilities;
using fr8.Infrastructure.Utilities.Configuration;
using Newtonsoft.Json;
using Hub.Infrastructure;
using HubWeb.Infrastructure_HubWeb;
using HubWeb.ViewModels.RequestParameters;
using Newtonsoft.Json.Linq;

namespace HubWeb.Controllers
{
    //[RoutePrefix("routes")]
    [Fr8ApiAuthorize]
    public class PlansController : ApiController
    {

        private readonly IPlan _plan;

        private readonly IActivityTemplate _activityTemplate;
        private readonly IActivity _activity;
        private readonly IFindObjectsPlan _findObjectsPlan;
        private readonly ISecurityServices _security;
        private readonly ICrateManager _crate;
        private readonly IPusherNotifier _pusherNotifier;
        private readonly IContainerService _container;
        private readonly IPlanTemplates _planTemplates;

        public PlansController()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
            _container = ObjectFactory.GetInstance<IContainerService>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _findObjectsPlan = ObjectFactory.GetInstance<IFindObjectsPlan>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _planTemplates = ObjectFactory.GetInstance<IPlanTemplates>();
        }

        //[HttpPost]
        //[Fr8HubWebHMACAuthenticate]
        //public async Task<IHttpActionResult> Create(Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, Guid? authorizationTokenId = null)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var userId = User.Identity.GetUserId();
        //        var result = await _activity.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, true, authorizationTokenId) as PlanDO;
        //        return Ok(Mapper.Map<PlanDTO>(result));
        //    }
        //}


        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        [HttpPost]
        public IHttpActionResult Post([FromBody] PlanEmptyDTO planDto,[FromUri] PlansPostParams parameters = null)
        {
            parameters = parameters ?? new PlansPostParams();

            if (!parameters.solution_name.IsNullOrEmpty())
            {
                return CreateSolution(parameters.solution_name).Result;
            }

            return Post(planDto, parameters.update_registrations);
        }

        [HttpPost]
        [NonAction]
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
        [NonAction]
        private IHttpActionResult Post(PlanEmptyDTO planDto, bool updateRegistrations = false)
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
                var result = PlanMappingHelper.MapPlanToDto(uow, _plan.GetFullPlan(uow, curPlanDO.Id));
                return Ok(result);
            }
        }

        

        [Fr8ApiAuthorize]
        //[ActionName("query")]
        [HttpGet]
        //formerly  GetByQuery
        public IHttpActionResult Query([FromUri] PlanQueryDTO planQuery)
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
        public IHttpActionResult Get([FromUri] PlansGetParams parameters)
        {
            if ((!parameters.name.IsNullOrEmpty() && parameters.id.HasValue)
                || (parameters.activity_id.HasValue && parameters.id.HasValue)
                || (parameters.activity_id.HasValue && !parameters.name.IsNullOrEmpty()))
            {
                return BadRequest("Multiple parameters are defined");
            }


            if (parameters.include_children && parameters.id.HasValue)
            {
                return GetFullPlan((Guid)parameters.id);
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

        //[Fr8ApiAuthorize]
        //[Route("full/{id:guid}")]
        //[ActionName("full")]
        [ResponseType(typeof(PlanDTO))]
        [HttpGet]
        [NonAction]
        public IHttpActionResult GetFullPlan(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = _plan.GetFullPlan(uow, id);
                var result = PlanMappingHelper.MapPlanToDto(uow, plan);

                return Ok(result);
            };
        }

        //[Route("getByAction/{id:guid}")]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(PlanDTO))]
        //[HttpGet]
        [NonAction]
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
        [Fr8HubWebHMACAuthenticate]
        //[HttpGet]
        [ResponseType(typeof(IEnumerable<PlanDTO>))]
        [NonAction]
        public IHttpActionResult GetByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlans = _plan.GetByName(uow, _security.GetCurrentAccount(uow), name, visibility);
                var fullPlans = curPlans.Select(curPlan => PlanMappingHelper.MapPlanToDto(uow, curPlan)).ToList();
                return Ok(fullPlans);

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
        //[Fr8ApiAuthorize]
        [NonAction]
        private IHttpActionResult Get(Guid? id = null)
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


        [HttpDelete]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult Delete(Guid id)
        {
            _plan.Delete(id);

            return Ok(id);
        }

        
        [HttpPost]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Deactivate(Guid planId)
        {
            await _plan.Deactivate(planId);
           
            return Ok();
        }

        /// <summary>
        /// Upload file with plan template and create plan from it. 
        /// </summary>
        /// /// <param name="planName">Name of newly created plan</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Upload(string planName)
        {
            IHttpActionResult result = InternalServerError();
            await Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((tsk) =>
            {
                MultipartMemoryStreamProvider prvdr = tsk.Result;

                foreach (HttpContent ctnt in prvdr.Contents)
                {
                    Stream stream = ctnt.ReadAsStreamAsync().Result;

                    var content = new StreamReader(stream).ReadToEnd();

                    var planTemplateDTO = JsonConvert.DeserializeObject<PlanTemplateDTO>(content);
                    planTemplateDTO.Name = planName;

                    result = this.Load(planTemplateDTO);
                }
            });

            return result;
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

        // Method for plan execution or continuation without payload specified
        [Fr8ApiAuthorize("Admin", "Customer")]
        [HttpGet]
        public async Task<IHttpActionResult> Run(Guid planId, Guid? containerId = null)
        {
            var result =  await _plan.Run(planId, null, containerId);

            if (result == null)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        // Method for plan execution  with payload
        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
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

        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        [ResponseType(typeof(PlanTemplateDTO))]
        [HttpPost]
        public async Task<IHttpActionResult> Templates(Guid planId)
        {
            var planTemplateDTO = _planTemplates.GetPlanTemplate(planId, User.Identity.GetUserId());

            return Ok(planTemplateDTO);
        }

        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public async Task<IHttpActionResult> Share(Guid planId)
        {
            var planTemplateDTO = _planTemplates.GetPlanTemplate(planId, User.Identity.GetUserId());

            var hmacService = ObjectFactory.GetInstance<IHMACService>();
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var dto = new PublishPlanTemplateDTO()
            {
                Name = planTemplateDTO.Name,
                Description = planTemplateDTO.Description,
                ParentPlanId = planId,
                PlanContents = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(planTemplateDTO))
            };

            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/plantemplates/");
            var headers = await hmacService.GenerateHMACHeader(
                uri,
                "PlanDirectory",
                CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
                User.Identity.GetUserId(),
                dto
            );

            await client.PostAsync<PublishPlanTemplateDTO>(uri, dto, headers: headers);

            return Ok();
        }

        [Fr8ApiAuthorize("Admin", "Customer", "Terminal")]
        [Fr8HubWebHMACAuthenticate]
        [HttpPost]
        public IHttpActionResult Load(PlanTemplateDTO dto)
        {
            try
            {
                var planDO = _planTemplates.LoadPlan(dto, User.Identity.GetUserId());
                return Ok(Mapper.Map<PlanEmptyDTO>(planDO));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}