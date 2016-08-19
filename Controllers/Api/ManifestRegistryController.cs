using System;
using System.Configuration;
using AutoMapper;
using Data.Interfaces;
using StructureMap;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;
using System.Web.Http.Description;
using System.Collections.Generic;
using System.Net;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Services;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers.Api
{
    public class ManifestRegistryController : ApiController
    {
        private readonly IManifestRegistryMonitor _manifestRegistryMonitor;
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IPusherNotifier _pusher;
        private readonly ISecurityServices _securityServices;
        private readonly IFr8Account _fr8Account;

        public ManifestRegistryController(
            IManifestRegistryMonitor manifestRegistryMonitor,
            IFr8Account fr8Account,
            IRestfulServiceClient restfulServiceClient,
            IUnitOfWorkFactory uowFactory,
            IPusherNotifier pusher,
            ISecurityServices securityServices)
        {
            if (manifestRegistryMonitor == null)
            {
                throw new ArgumentNullException(nameof(manifestRegistryMonitor));
            }

            _manifestRegistryMonitor = manifestRegistryMonitor;
            _fr8Account = fr8Account;
            _restfulServiceClient = restfulServiceClient;
            _uowFactory = uowFactory;
            _pusher = pusher;
            _securityServices = securityServices;
        }

        /// <summary>
        /// Retrieves the collection of manifests registered in the hub
        /// </summary>
        /// <response code="200">Collection of registered manifests</response>
        [HttpGet]
        [ResponseType(typeof(List<ManifestDescriptionDTO>))]
        public IHttpActionResult Get()
        {
            using (var uow = _uowFactory.Create())
            {
                var systemUserAccount = _fr8Account.GetSystemUser();
                if (systemUserAccount == null)
                    return BadRequest("System Account is missing");

                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccount.UserName);
                var list = manifestDescriptions.Select(m => new ManifestDescriptionDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Version = m.Version,
                    SampleJSON = m.SampleJSON,
                    Description = m.Description,
                    RegisteredBy = m.RegisteredBy
                }).ToList();

                return Ok(list);
            }
        }

        [HttpGet]
        [ActionName("get_manifest_page_url")]
        public async Task<IHttpActionResult> GetManifestPageUrl(string manifestName)
        {
            return Ok(await _restfulServiceClient.PostAsync<string, string>(
                new Uri($"{CloudConfigurationManager.GetSetting("PlanDirectoryUrl")}/api/v1/page_generation/generate_manifest_page"),
                manifestName));
        }

        /// <summary>
        /// Retrieves URL of Google Form that is used to submit new manifests
        /// </summary>
        /// <response code="200">URL of Google Form</response>
        [HttpGet]
        [ResponseType(typeof(string))]
        public IHttpActionResult Submit()
        {
            return Ok(ConfigurationManager.AppSettings["ManifestSubmissionFormUrl"]);
        }
        /// <summary>
        /// Submits new manifest to manifest registry
        /// </summary>
        /// <remarks>
        /// if ManifestRegistryParams.version is empty gets the manifest description with given name and greatest version. Otherwise checks if there is any manifest description with given name and given version</remarks>
        /// <param name="description">Description of new manifest</param>
        /// <response code="200">Description of the submitted manifest</response>
        [HttpPost]
        [ResponseType(typeof(ManifestDescriptionDTO))]
        [DockyardAuthorize(Roles = Roles.Admin)]
        public IHttpActionResult Post(ManifestDescriptionDTO description)
        {
            ManifestDescriptionCM manifestDescription = Mapper.Map<ManifestDescriptionCM>(description);
            manifestDescription.Id = NextId();
            var systemUserAccount = _fr8Account.GetSystemUser();
            if (systemUserAccount == null)
                return BadRequest("System Account is Missing");

            using (var uow = _uowFactory.Create())
            {
                uow.MultiTenantObjectRepository.Add(manifestDescription, systemUserAccount.UserName);

                uow.SaveChanges();
            }

            var model = Mapper.Map<ManifestDescriptionDTO>(manifestDescription);

            return Ok(model);
        }
        /// <summary>
        /// Starts internal plan that monitors new manifest submissions and creates a JIRA for them
        /// </summary>
        [HttpPost]
        [DockyardAuthorize(Roles = Roles.Admin)]
        [SwaggerResponse(HttpStatusCode.OK, "Manifest monitoring plan was successfully launched")]
        [SwaggerResponseRemoveDefaults]
        public async Task<IHttpActionResult> RunMonitoring()
        {
            //We don't need to wait for the result as the purpose of this method is just to initiate a start (as a double-check measure)
            var currentUser = _securityServices.GetCurrentUser();
            _pusher.NotifyUser(new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericInfo,
                Subject = "Manifest Registry Monitoring",
                Message = "Preparing to launch manifest registry monitoring...",
                Collapsed = false
            }, currentUser);
            var resultNotification = new NotificationMessageDTO
            {
                NotificationType = NotificationType.GenericSuccess,
                Subject = "Manifest Registry Monitoring",
                Collapsed = true
            };
            ManifestRegistryMonitorResult result;
            try
            {
                result = await _manifestRegistryMonitor.StartMonitoringManifestRegistrySubmissions();
                resultNotification.Message = result.IsNewPlan
                    ? $"New plan with Id {result.PlanId} was created and started to monitor manifest registry"
                    : $"An existing plan with Id {result.PlanId} was used to monitor manifest registry";
            }
            catch (Exception ex)
            {
                resultNotification.NotificationType = NotificationType.GenericFailure;
                resultNotification.Message = $"Failed to manually start manifest registry monitoring. Reason - {ex.Message}. See incidents and error logs for more details";
                Logger.GetLogger().Error("Failed to manually start manifest registry monitoring", ex);
                throw;
            }
            finally
            {
                _pusher.NotifyUser(resultNotification, currentUser);
            }
            return Ok();
        }

        private string NextId()
        {
            int result = 1;
            using (var uow = _uowFactory.Create())
            {
                var systemUserAccount = _fr8Account.GetSystemUser();
                if (systemUserAccount == null)
                    throw new ApplicationException("System Account is Missing");

                var manifestDescriptions = uow.MultiTenantObjectRepository.AsQueryable<ManifestDescriptionCM>(systemUserAccount.UserName);
                if (!manifestDescriptions.Any())
                {
                    return result.ToString();
                }
                result = manifestDescriptions.Select(x => int.Parse(x.Id)).OrderByDescending(x => x).First() + 1;
            }
            return result.ToString();
        }
    }
}
