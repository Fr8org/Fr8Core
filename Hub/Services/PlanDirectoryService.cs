using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Plan;
using Data.States;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Helper;
using Hub.Interfaces;
using Newtonsoft.Json.Linq;

namespace Hub.Services
{
    public class PlanDirectoryService : IPlanDirectoryService
    {
        private readonly IHMACService _hmacService;
        private readonly IRestfulServiceClient _client;
        private readonly IPusherNotifier _pusherNotifier;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPlan _planService;
        private readonly IActivityTemplate _activityTemplate;

        private readonly IPlanTemplate _planTemplate;
        private readonly ISearchProvider _searchProvider;
        private readonly IWebservicesPageGenerator _webservicesPageGenerator;
        private readonly IPlanTemplateDetailsGenerator _planTemplateDetailsGenerator;



        public PlanDirectoryService(IHMACService hmac, 
                                    IRestfulServiceClient client, 
                                    IPusherNotifier pusherNotifier,
                                    IUnitOfWorkFactory unitOfWorkFactory, 
                                    IPlan planService, 
                                    IActivityTemplate activityTemplate,
                                    IPlanTemplate planTemplate,
                                    ISearchProvider searchProvider,
                                    IWebservicesPageGenerator webservicesPageGenerator,
                                    IPlanTemplateDetailsGenerator planTemplateDetailsGenerator)
        {
            _hmacService = hmac;
            _client = client;
            _pusherNotifier = pusherNotifier;
            _unitOfWorkFactory = unitOfWorkFactory;
            _planService = planService;
            _activityTemplate = activityTemplate;

            _planTemplate = planTemplate;
            _searchProvider = searchProvider;
            _webservicesPageGenerator = webservicesPageGenerator;
            _planTemplateDetailsGenerator = planTemplateDetailsGenerator;
        }

        public async Task<string> GetToken(string UserId)
        {
            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/v1/authentication/token");
            var headers =
                await
                    _hmacService.GenerateHMACHeader(uri, "PlanDirectory",
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret"), UserId);

            var json = await _client.PostAsync<JObject>(uri, headers: headers);
            var token = json.Value<string>("token");

            return token;
        }

        public string LogOutUrl()
        {
            return CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/Home/LogoutByToken";
        }

        public async Task<PublishPlanTemplateDTO> GetTemplate(Guid id, string userId)
        {
            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/v1/plan_templates?id=" + id);
            var headers = await _hmacService.GenerateHMACHeader(
                uri,
                "PlanDirectory",
                CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
                userId
            );

            try
            {
                return await _client.GetAsync<PublishPlanTemplateDTO>(uri, headers: headers);
            }
            catch (Fr8.Infrastructure.Communication.RestfulServiceException)
            {
                return null;
            }
        }

        public async Task Share(Guid planId, string userId)
        {
            var planDto = CrateTemplate(planId, userId);
            
            var dto = new PublishPlanTemplateDTO
            {
                Name = planDto.Name,
                Description = planDto.Description,
                ParentPlanId = planId,
                PlanContents = planDto
            };

            var planTemplateCM = await _planTemplate.CreateOrUpdate(userId, dto);
            await _searchProvider.CreateOrUpdate(planTemplateCM);
            await _webservicesPageGenerator.Generate(planTemplateCM, userId);
            await _planTemplateDetailsGenerator.Generate(dto);


            // @tony.yakovets: for now i left this request to itself because classes above to tight coupled to PlanDirectory project
            // even if it is a hub
            //var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/v1/plan_templates/");
            //var headers = await _hmacService.GenerateHMACHeader(
            //    uri,
            //    "PlanDirectory",
            //    CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
            //    userId,
            //    dto
            //);

            //await _client.PostAsync(uri, dto, headers: headers);

            // Notify user with directing him to PlanDirectory with related search query
            var url = CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/plan_directory#?planSearch=" + HttpUtility.UrlEncode(dto.Name);
            _pusherNotifier.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericSuccess,
                Subject = "Success",
                Message = $"Plan Shared. To view, click on " + url,
                Collapsed = false
            }, userId);
        }

        public async Task Unpublish(Guid planId, string userId, bool privileged)
        {
            //TODO: add security check with new security model
            //var identity = User.Identity as ClaimsIdentity;
            //var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");
            //var fr8AccountId = identity.GetUserId();

            var planTemplateCM = await _planTemplate.Get(userId, planId);

            if (planTemplateCM != null)
            {
                if (planTemplateCM.OwnerId != userId && !privileged)
                {
                    throw new UnauthorizedAccessException(); // Unauthorized();
                }
                await _planTemplate.Remove(userId, planId);
                await _searchProvider.Remove(planId);
            }


            //var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/v1/plan_templates/?id=" + planId);
            //var headers = await _hmacService.GenerateHMACHeader(
            //    uri,
            //    "PlanDirectory",
            //    CloudConfigurationManager.GetSetting("PlanDirectorySecret"),
            //    userId
            //);

            //await _client.DeleteAsync(uri, headers: headers);

            // Notify user that plan successfully deleted
            _pusherNotifier.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericSuccess,
                Subject = "Success", 
                Message = $"Plan Unpublished.",
                Collapsed = false
            }, userId);
        }

        public PlanDTO CrateTemplate(Guid planId, string userId)
        {
            PlanDO clonedPlan;

            using (var uow = _unitOfWorkFactory.Create())
            {
                var plan = _planService.GetPlanByActivityId(uow, planId);

                clonedPlan = (PlanDO) PlanTreeHelper.CloneWithStructure(plan);
            }

            clonedPlan.PlanState = PlanState.Inactive;

            //linearlize tree structure
            var planTree = clonedPlan.GetDescendants();
            var idsMap = new Dictionary<Guid, Guid>();

            foreach (var planNodeDO in planTree)
            {
                var oldId = planNodeDO.Id;

                planNodeDO.Id = Guid.NewGuid();
                planNodeDO.Fr8Account = null;
                planNodeDO.Fr8AccountId = null;

                var activity = planNodeDO as ActivityDO;

                if (activity != null)
                {
                    activity.AuthorizationToken = null;
                }

                idsMap.Add(oldId, planNodeDO.Id);
            }

            foreach (var activity in planTree.OfType<ActivityDO>())
            {
                activity.CrateStorage = UpdateCrateStorage(activity.CrateStorage, idsMap);
            }
            
            return PlanMappingHelper.MapPlanToDto(clonedPlan);
        }

        private string UpdateCrateStorage(string crateStorage, Dictionary<Guid, Guid> idsMap)
        {
            if (string.IsNullOrWhiteSpace(crateStorage))
            {
                return crateStorage;
            }

            var crateStorageBuilder = new StringBuilder(crateStorage);

            foreach (var idMap in idsMap)
            {
                crateStorageBuilder.Replace(idMap.Key.ToString("D"), idMap.Value.ToString("D"));
            }

            return crateStorage;
        }


        public PlanNoChildrenDTO CreateFromTemplate(PlanDTO plan, string userId)
        {
            var planDo = new PlanDO()
            {
                Category = plan.Category,
                CreateDate = DateTimeOffset.Now,
                Fr8AccountId = userId,
                Id = Guid.NewGuid(),
                PlanState = PlanState.Inactive,
                Description = plan.Description,
                Name = plan.Name,
                Visibility = PlanVisibility.Standard,
            };

            var idsMap = new Dictionary<Guid, Guid>();

            idsMap[plan.Id] = planDo.Id;

            if (plan.SubPlans != null)
            {
                foreach (var fullSubplanDto in plan.SubPlans)
                {
                    var subplan = new SubplanDO(plan.StartingSubPlanId == fullSubplanDto.SubPlanId);

                    subplan.Id = Guid.NewGuid();
                    subplan.Name = fullSubplanDto.Name;
                    subplan.Fr8AccountId = userId;

                    if (fullSubplanDto.SubPlanId != null)
                    {
                        idsMap[fullSubplanDto.SubPlanId.Value] = subplan.Id;
                    }

                    planDo.AddChildWithDefaultOrdering(subplan);

                    if (fullSubplanDto.Activities != null)
                    {
                        foreach (var activityDto in fullSubplanDto.Activities)
                        {
                            var activity = AutoMapper.Mapper.Map<ActivityDO>(activityDto);

                            foreach (var descendant in activity.GetDescendants())
                            {
                                var oldId = descendant.Id;

                                descendant.Id = Guid.NewGuid();
                                idsMap[oldId] = descendant.Id;
                            }

                            subplan.AddChild(activity, activityDto.Ordering);
                        }
                    }
                }

                foreach (var activity in planDo.GetDescendants().OfType<ActivityDO>())
                {
                    ActivityTemplateDO activityTemplate;

                    if (!_activityTemplate.TryGetByKey(activity.ActivityTemplateId, out activityTemplate))
                    {
                        throw new KeyNotFoundException($"Activity '{activity.Id}' use activity template '{activity.ActivityTemplate?.Name}' with id = '{activity.ActivityTemplateId}' that is unknown to this Hub");
                    }

                    activity.CrateStorage = UpdateCrateStorage(activity.CrateStorage, idsMap);
                }
            }

            using (var uow = _unitOfWorkFactory.Create())
            {
                uow.PlanRepository.Add(planDo);
                uow.SaveChanges();


                return AutoMapper.Mapper.Map<PlanNoChildrenDTO>(planDo);
            }
        }
    }
}
