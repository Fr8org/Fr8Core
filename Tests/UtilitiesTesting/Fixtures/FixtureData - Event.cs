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
        public static EventCM TestEmptyEventDto()
        {
            return new EventCM { EventName = string.Empty };
        }

        public static EventCM TestTerminalIncidentDto()
        {

            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingDataCm
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventCM
            {
                EventName = "Terminal Incident",
            };

            eventDto.CrateStorage.Add(loggingDataCrate);

            return eventDto;
        }

        public static EventCM TestTerminalEventDto()
        {
            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingDataCm
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventCM
            {
                EventName = "Terminal Event",
            };

            eventDto.CrateStorage.Add(loggingDataCrate);

            return eventDto;
        }
    }
}
