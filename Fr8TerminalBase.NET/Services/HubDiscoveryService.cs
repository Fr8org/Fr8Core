using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using log4net;
using StructureMap;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that allows to gain access to multiple Hubs in those where the current terminal is registered.
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IHubDiscoveryService.md
    /// </summary>
    public class HubDiscoveryService : IHubDiscoveryService
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IRestfulServiceClientFactory _restfulServiceClientFactory;
        private readonly IActivityStore _activityStore;
        private readonly IRetryPolicy _hubDiscoveryRetryPolicy;
        private readonly Dictionary<string, TaskCompletionSource<string>> _hubSecrets = new Dictionary<string, TaskCompletionSource<string>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly string _masterHubUrl;
        private readonly HashSet<string> _bindedHubs = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly AsyncMultiLock _hubListLock = new AsyncMultiLock();
        private readonly string _apiSuffix;
        private bool _hubListQueried;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public HubDiscoveryService(IRestfulServiceClientFactory restfulServiceClientFactory, IActivityStore activityStore, IRetryPolicy hubDiscoveryRetryPolicy)
        {
            _restfulServiceClientFactory = restfulServiceClientFactory;
            _restfulServiceClient = _restfulServiceClientFactory.Create(null);
            _activityStore = activityStore;
            _hubDiscoveryRetryPolicy = hubDiscoveryRetryPolicy;
            _apiSuffix = $"/api/{CloudConfigurationManager.GetSetting("HubApiVersion")}";
            _masterHubUrl = CloudConfigurationManager.GetSetting("DefaultHubUrl");
        }

        /**********************************************************************************/

        public async Task<IHubCommunicator> GetHubCommunicator(string hubUrl)
        {
            TaskCompletionSource<string> setSecretTask;
            var originalUrl = hubUrl;

            hubUrl = NormalizeHubUrl(hubUrl);

            if (string.IsNullOrWhiteSpace(hubUrl))
            {
                throw new ArgumentException($"Invalid hub url: {originalUrl}", nameof(hubUrl));
            }

            lock (_hubSecrets)
            {
                if (!_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                {
                    setSecretTask = new TaskCompletionSource<string>();
                    _hubSecrets[hubUrl] = setSecretTask;

#pragma warning disable 4014
                    Task.Factory.StartNew(() => RequestDiscoveryTask(hubUrl));
#pragma warning restore 4014
                }
            }
            
            var secret = await setSecretTask.Task;
            var restfulServiceClient = _restfulServiceClientFactory.Create(new HubAuthenticationHeaderSignature(secret, null));
            return new DefaultHubCommunicator(restfulServiceClient, string.Concat(hubUrl, _apiSuffix), secret, null);
        }

        /**********************************************************************************/

        public Task<IHubCommunicator> GetMasterHubCommunicator()
        {
            return GetHubCommunicator(_masterHubUrl);
        }

        /**********************************************************************************/

        public void SetHubSecret(string hubUrl, string secret)
        {
            Logger.Info($"Received the secret for Hub at '{hubUrl}'.");

            TaskCompletionSource<string> setSecretTask;
            var originalUrl = hubUrl;

            hubUrl = NormalizeHubUrl(hubUrl);

            if (string.IsNullOrWhiteSpace(hubUrl))
            {
                throw new ArgumentException($"Invalid hub url: {originalUrl}", nameof(hubUrl));
            }

            lock (_hubSecrets)
            {
                if (!_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                {
                    setSecretTask = new TaskCompletionSource<string>();
                    _hubSecrets[hubUrl] = setSecretTask;
                }
            }

            SubscribeToHub(hubUrl);

            if (!setSecretTask.TrySetResult(secret))
            {
                // may be we already set the result or previous secret resultion taks failed
                lock (_hubSecrets)
                {
                    setSecretTask = new TaskCompletionSource<string>();
                    _hubSecrets[hubUrl] = setSecretTask;
                    setSecretTask.SetResult(secret);
                }
            }
        }

        /**********************************************************************************/

        public async Task<string[]> GetSubscribedHubs()
        {
            var hubList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            using (await _hubListLock.Lock(_bindedHubs))
            {
                if (_hubListQueried)
                {
                    lock (_bindedHubs)
                    {
                        return _bindedHubs.ToArray();
                    }
                }

                var hubCommunicator = await GetMasterHubCommunicator();
                var hubs = await hubCommunicator.QueryWarehouse<HubSubscriptionCM>(new List<FilterConditionDTO>());

                if (hubs != null)
                {
                    foreach (var hubSubscriptionCm in hubs)
                    {
                        hubList.Add(hubSubscriptionCm.HubUrl);
                    }
                }

                hubList.Add(NormalizeHubUrl(_masterHubUrl));
                _hubListQueried = true;

                lock (_bindedHubs)
                {
                    foreach (var hub in hubList)
                    {
                        _bindedHubs.Add(hub);
                    }
                }
            }

            return hubList.ToArray();
        }

        /**********************************************************************************/

        private void SubscribeToHub(string hubUrl)
        {
            lock (_bindedHubs)
            {
                if (string.IsNullOrWhiteSpace(hubUrl) || !_bindedHubs.Add(hubUrl))
                {
                    return;
                }
            }

#pragma warning disable 4014
            Task.Factory.StartNew(() => SubsribeToHubTask(hubUrl));
#pragma warning restore 4014
        }

        /**********************************************************************************/

        private void UnsubscribeFromHub(string hubUrl)
        {
            hubUrl = NormalizeHubUrl(hubUrl);

            lock (_bindedHubs)
            {
                if (string.IsNullOrWhiteSpace(hubUrl) || !_bindedHubs.Remove(hubUrl))
                {
                    return;
                }
            }

#pragma warning disable 4014
            Task.Factory.StartNew(() => UnsubscribeFromHubTask(hubUrl));
#pragma warning restore 4014
        }

        /**********************************************************************************/

        private async Task SubsribeToHubTask(string hubUrl)
        {
            try
            {
                Logger.Info($"Terminal '{_activityStore.Terminal.Name}' wants to add Hub at '{hubUrl}' to subscription list");

                var masterHubCommunicator = await GetMasterHubCommunicator();
                await masterHubCommunicator.AddOrUpdateWarehouse(new HubSubscriptionCM(hubUrl.ToLower()));

                Logger.Info($"Terminal '{_activityStore.Terminal.Name}' sucessfully added Hub '{hubUrl}' to subscription list");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add hub '{hubUrl}' to the subscription list of terminal {_activityStore.Terminal.Name} ({_activityStore.Terminal.Version})", ex);
            }
        }

        /**********************************************************************************/

        private async Task UnsubscribeFromHubTask(string hubUrl)
        {
            try
            {
                Logger.Info($"Terminal '{_activityStore.Terminal.Name}' wants to remove Hub at '{hubUrl}' from subscription list");
                var masterHubCommunicator = await GetMasterHubCommunicator();
                await masterHubCommunicator.DeleteFromWarehouse<HubSubscriptionCM>(new List<FilterConditionDTO>
                {
                    new FilterConditionDTO
                    {
                        Field = nameof(HubSubscriptionCM.HubUrl),
                        Operator = "eq",
                        Value = hubUrl.ToLower()
                    }
                });
                Logger.Info($"Terminal '{_activityStore.Terminal.Name}' sucessfully removed Hub '{hubUrl}' from subscription list");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to remove hub '{hubUrl}' from the subscription list of terminal {_activityStore.Terminal.Name} ({_activityStore.Terminal.Version})", ex);
            }
        }
        
        /**********************************************************************************/

        private async Task RequestDiscoveryTask(string hubUrl)
        {
            Exception lastException = null;

            try
            {
                Logger.Info($"Terminal {_activityStore.Terminal.Name} is requesting discovery for endpoint '{_activityStore.Terminal.Endpoint}' from Hub at '{hubUrl}' ");

                var response = await _hubDiscoveryRetryPolicy.Do(() => _restfulServiceClient.PostAsync<TerminalDTO, ResponseMessageDTO>(new Uri(string.Concat(hubUrl, _apiSuffix, "/terminals/forceDiscover")), _activityStore.Terminal, (string) null));
                
                if (!string.IsNullOrWhiteSpace(response?.ErrorCode))
                {
                    lastException = new Exception(response.Message);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            if (lastException != null)
            {
                lock (_hubSecrets)
                {
                    TaskCompletionSource<string> setSecretTask;

                    if (_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                    {
                        Logger.Error($"Terminal {_activityStore.Terminal.Name}({_activityStore.Terminal.Version}): Hub at '{hubUrl}' refused to call terminal discovery endpoint: {lastException.Message}");
                        setSecretTask.TrySetException(new Exception($"Failed to request discovery from the Hub at : {hubUrl}", lastException));
                    }
                }

                UnsubscribeFromHub(hubUrl);
            }
            else
            {
#pragma warning disable 4014
                Task.Run(async () =>
                {
                    bool shouldUnubscribe = false;

                    await Task.Delay(5000);

                    lock (_hubSecrets)
                    {
                        TaskCompletionSource<string> setSecretTask;

                        if (_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                        {
                            shouldUnubscribe = setSecretTask.TrySetException(new Exception($"Terminal { _activityStore.Terminal.Name }({ _activityStore.Terminal.Version}): Hub '{hubUrl}' failed to respond with discovery request within a given period of time."));
                        }
                    }

                    if (shouldUnubscribe)
                    {
                        Logger.Error($"Terminal { _activityStore.Terminal.Name }({ _activityStore.Terminal.Version}): Hub at '{hubUrl}'failed to respond with discovery request within a given period of time");
                        UnsubscribeFromHub(hubUrl);
                    }
                });
#pragma warning restore 4014
            }
        }

        /**********************************************************************************/

        private string NormalizeHubUrl(string url)
        {
            //trim api + version specifications
            var apiIndex = url.LastIndexOf("api", StringComparison.InvariantCultureIgnoreCase);

            if (apiIndex > 0)
            {
                url = url.Substring(0, apiIndex);
            }

            var schemaIndex = url.IndexOf("://", StringComparison.InvariantCulture);

            if (schemaIndex < 0)
            {
                url = "http://" + url.TrimStart('/', '\\');
            }

            return url.TrimEnd('/', '\\');
        }

        /**********************************************************************************/
    }
}
