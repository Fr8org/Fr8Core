
using System;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure;

namespace Core.Services
{
    /// <summary>
    /// Event service implementation
    /// </summary>
    public class Event : IEvent
    {
        /// <see cref="IEvent.HandlePluginIncident"/>
        public void HandlePluginIncident(HistoryItemDO incident)
        {
            EventManager.ReportPluginIncident(incident);
        }
    }
}
