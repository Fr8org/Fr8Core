using System.Collections.Generic;
using Data.Crates;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using StructureMap;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static EventDTO TestEmptyEventDto()
        {
            return new EventDTO { EventName = string.Empty };
        }

        public static EventDTO TestPluginIncidentDto()
        {

            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingData
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventDTO
            {
                EventName = "Plugin Incident",
            };

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage((() => eventDto.CrateStorage)))
            {
                updater.CrateStorage.Add(loggingDataCrate);
            }

            return eventDto;
        }

        public static EventDTO TestPluginEventDto()
        {
            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingData
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventDTO
            {
                EventName = "Plugin Event",
            };

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage((() => eventDto.CrateStorage)))
            {
                updater.CrateStorage.Add(loggingDataCrate);
            }

            return eventDto;
        }
    }
}
