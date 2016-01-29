using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalFr8Core.Managers
{
    public class EventManager
    {
        public StandardLoggingCM RouteActivated(EventLoggingDTO eventLogging)
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();
            standardLoggingCM.LoggingMTkey = Guid.NewGuid().ToString();

            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = eventLogging.CustomerId,
                Manufacturer = "Fr8 Company",
                Data = eventLogging.Data,
                PrimaryCategory = "Plan",
                SecondaryCategory = "RouteState",
                Activity = "StateChanged",
                Status = eventLogging.Status,
                CreateDate = new DateTime(),
                Type = "FactDO",
                Name = eventLogging.EventName,
                IsLogged = false,
                ObjectId = eventLogging.ObjectId
            };

            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }

        public StandardLoggingCM ContainerLaunched(EventLoggingDTO eventLogging)
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();

            standardLoggingCM.LoggingMTkey = Guid.NewGuid().ToString();
            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = eventLogging.CustomerId,
                Manufacturer = "Fr8 Company",
                Data = eventLogging.Data,
                PrimaryCategory = "Container",
                SecondaryCategory = "Operations",
                Activity = "Launched",
                Status = eventLogging.Status,
                CreateDate = new DateTime(),
                Type = "FactDO",
                Name = eventLogging.EventName,
                ObjectId = eventLogging.ObjectId,
                IsLogged = false
            };
            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }

        public StandardLoggingCM RouteDeactivated(EventLoggingDTO eventLogging)
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();
            standardLoggingCM.LoggingMTkey = Guid.NewGuid().ToString();

            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = eventLogging.CustomerId,
                Manufacturer = "Fr8 Company",
                Data = eventLogging.Data,
                PrimaryCategory = "Plan",
                SecondaryCategory = "RouteState",
                Activity = "StateChanged",
                Status = eventLogging.Status,
                CreateDate = new DateTime(),
                Type = "FactDO",
                Name = eventLogging.EventName,
                ObjectId = eventLogging.ObjectId,
                IsLogged = false
            };
            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }

        public StandardLoggingCM ContainerExecutionComplete(EventLoggingDTO eventLogging)
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();
            standardLoggingCM.LoggingMTkey = Guid.NewGuid().ToString();

            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = eventLogging.CustomerId,
                Manufacturer = "Fr8 Company",
                Data = eventLogging.Data,
                PrimaryCategory = "Container Execution",
                SecondaryCategory = "Container",
                Activity = "Launched",
                Status = eventLogging.Status,
                CreateDate = new DateTime(),
                Type = "FactDO",
                Name = eventLogging.EventName,
                ObjectId = eventLogging.ObjectId,
                IsLogged = false
            };
            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }

        public StandardLoggingCM ActionExecuted(EventLoggingDTO eventLogging)
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();
            standardLoggingCM.LoggingMTkey = Guid.NewGuid().ToString();

            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = eventLogging.CustomerId,
                Manufacturer = "Fr8 Company",
                Data = eventLogging.Data,
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Action",
                Activity = "Dispatched",
                Status = eventLogging.Status,
                CreateDate = new DateTime(),
                Type = "FactDO",
                IsLogged = false,
                Name = eventLogging.EventName,
                ObjectId = eventLogging.ObjectId
            };
            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }
    }
}