using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    class HubDiscoveryService : IHubDiscoveryService
    {
        /*private class HubDiscoveryInfo
        {
            public 
        }*/

        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IHMACService _hmacService;
        private readonly IActivityStore _activityStore;
        private readonly Dictionary<string, TaskCompletionSource<string>> _hubSecrets = new Dictionary<string, TaskCompletionSource<string>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> _pendingDiscoveries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly string _masterHubUrl;

        public HubDiscoveryService(IRestfulServiceClient restfulServiceClient, IHMACService hmacService, IActivityStore activityStore)
        {
            _restfulServiceClient = restfulServiceClient;
            _hmacService = hmacService;
            _activityStore = activityStore;
            _masterHubUrl = $"{CloudConfigurationManager.GetSetting("CoreWebServerUrl")}api/{CloudConfigurationManager.GetSetting("HubApiVersion")}";
            
            // TODO: remove this code. Terminal secret should be issued by the Hub during discovery
            // SetHubSecret(_masterHubUrl, CloudConfigurationManager.GetSetting("TerminalSecret") ?? ConfigurationManager.AppSettings[activityStore.Terminal.Name + "TerminalSecret"]);
        }

        public async Task<IHubCommunicator> GetHubCommunicator(string hubUrl)
        {
            TaskCompletionSource<string> setSecretTask;
            var searchUrl = NormalizeHubUrl(hubUrl);

            lock (_hubSecrets)
            {
                if (!_hubSecrets.TryGetValue(searchUrl, out setSecretTask))
                {
                    setSecretTask = new TaskCompletionSource<string>();
                    _hubSecrets[searchUrl] = setSecretTask;
                }
            }

            lock (_pendingDiscoveries)
            {
                if (!_pendingDiscoveries.Contains(searchUrl))
                {
                    _pendingDiscoveries.Add(searchUrl);
#pragma warning disable 4014
                    Task.Factory.StartNew(() => RequestDiscovery(hubUrl));
#pragma warning restore 4014
                }
            }

            var secret = await setSecretTask.Task;

            return new DefaultHubCommunicator(_restfulServiceClient, _hmacService, hubUrl, _activityStore.Terminal.PublicIdentifier, secret);
        }

        public Task<IHubCommunicator> GetMasterHubCommunicator()
        {
            return GetHubCommunicator(_masterHubUrl);
        }

        public void SetHubSecret(string hubUrl, string secret)
        {
            TaskCompletionSource<string> setSecretTask;

            hubUrl = NormalizeHubUrl(hubUrl);

            lock (_hubSecrets)
            {
                if (!_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                {
                    setSecretTask = new TaskCompletionSource<string>();
                    _hubSecrets[hubUrl] = setSecretTask;
                }
            }

            lock (_pendingDiscoveries)
            {
                _pendingDiscoveries.Remove(hubUrl);
            }

            setSecretTask.TrySetResult(secret);
        }

        public async Task RequestDiscovery()
        {
            await RequestDiscovery(_masterHubUrl);
        }

        private async Task RequestDiscovery(string hubUrl)
        {
            Exception lastException = null;

            for (int i = 0; i < 5; i++)
            {
                lock (_pendingDiscoveries)
                {
                    if (!_pendingDiscoveries.Contains(hubUrl))
                    {
                        return;
                    }
                }

                try
                {
                    await _restfulServiceClient.PostAsync(new Uri(hubUrl + "/terminals/discover"), null, _activityStore.Terminal.Endpoint);

                    lock (_pendingDiscoveries)
                    {
                        _pendingDiscoveries.Remove(hubUrl);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                await Task.Delay(2000);
            }

            lock (_hubSecrets)
            {
                TaskCompletionSource<string> setSecretTask;

                if (_hubSecrets.TryGetValue(hubUrl, out setSecretTask))
                {
                    setSecretTask.SetException(new Exception($"Failed to request discovery from the Hub at : {hubUrl}", lastException));
                }
            }
        }

        private string NormalizeHubUrl(string url)
        {
            //trim api + version specifications
            var apiIndex = url.LastIndexOf("api", StringComparison.InvariantCultureIgnoreCase);

            if (apiIndex > 0)
            {
                url = url.Substring(0, apiIndex);
            }

            return url.TrimEnd('/', '\\');
        }
    }
}
