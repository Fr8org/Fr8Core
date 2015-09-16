
using System;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {
        /// <see cref="IEvent.HandlePluginIncident"/>
        public void HandlePluginIncident(LoggingData incident)
        {
            EventManager.ReportPluginIncident(incident);
        }

        public void HandlePluginEvent(LoggingData eventData)
        {
            EventManager.ReportPluginEvent(eventData);
        }

        public string GetPluginUrl(string curPluginName, string curPluginVersion)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IPluginDO curPlugin =
                    uow.PluginRepository.FindOne(
                        plugin => plugin.Name.Equals(curPluginName) && plugin.Version.Equals(curPluginVersion));

                return (curPlugin != null) ? curPlugin.Endpoint : string.Empty;
            }
        }

        public void Process(string curExternalEventPayload)
        {
            throw new NotImplementedException();
        }
    }
}
