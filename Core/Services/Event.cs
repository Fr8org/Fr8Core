
using System;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure;

namespace Core.Services
{
    public class Event : IEventService
    {
        public bool HandlePluginIncident(HistoryItemDO incident)
        {
            try
            {
                EventManager.ReportPluginIncident(incident);

                return true;
            }
            catch (Exception)
            {
                //Nothing doing by cathing the exception. Just return to mention internal server error.
                return false;
            }
        }
    }
}
