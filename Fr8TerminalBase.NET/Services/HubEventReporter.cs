using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.TerminalBase.Interfaces;
using log4net;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that allows to broadcast event crates to multiple Hubs where current terminal is registered.
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IHubEventReporter.md
    /// </summary>
    public class HubEventReporter : IHubEventReporter
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        private readonly IHubDiscoveryService _hubDiscovery;
        private readonly IActivityStore _activityStore;

        public HubEventReporter(IHubDiscoveryService hubDiscovery, IActivityStore activityStore)
        {
            _hubDiscovery = hubDiscovery;
            _activityStore = activityStore;
        }

        public async Task Broadcast(Crate eventPayload)
        {
            var hubList = await _hubDiscovery.GetSubscribedHubs();
            var tasks = new List<Task>();
            
            foreach (var hubUrl in hubList)
            {
                tasks.Add(NotifyHub(hubUrl, eventPayload));
            }

            await Task.WhenAll(tasks);
        }

        private async Task NotifyHub(string hubUrl, Crate eventPayload)
        {
            try
            {
                Logger.Info($"Terminal at '{_activityStore.Terminal?.Endpoint}' is sedning event to Hub at '{hubUrl}'.");
                var hubCommunicator = await _hubDiscovery.GetHubCommunicator(hubUrl);
                await hubCommunicator.SendEvent(eventPayload);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send event to hub '{hubUrl}'", ex);
            }
        }

        public Task<IHubCommunicator> GetMasterHubCommunicator()
        {
            return _hubDiscovery.GetMasterHubCommunicator();
        }
    }
}
