using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces;
using Utilities.Configuration.Azure;
using Data.Infrastructure;
using Utilities.Logging;
using Data.Entities;
using System.Net.Http;

namespace Hub.Managers
{
    public class Event
    {
        public static async Task Publish(string eventName, string customerId, string objectId, string data, string status)
        {
            var jsonObject = JsonConvert.SerializeObject(new { eventName, customerId, objectId, data, status }).ToString();
            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var terminalName = "terminalFr8Core";
            TerminalDO terminalDO;

            // we are not able to get terminal in Hub project. take terminal endpoint from terminal
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalDO = uow.TerminalRepository.GetQuery().FirstOrDefault(d => d.Name == terminalName);
                LogEvent(jsonObject.ToString(), "Fr8 Internal Activity");

                if (terminalDO != null)
                {
                    await
                        restClient.PostAsync<object>(
                            new Uri("http://" + terminalDO.Endpoint + "/terminals/" + terminalDO.Name + "/events"),
                            new StringContent(jsonObject,
                             Encoding.UTF8, "application/json"));
                }
            }
        }

        public static void LogEvent(string data, string label)
        {
            string message = string.Format("Event {0} generated with and Data = {1}", label, data);
            Logger.GetLogger().Info(message);
        }
    }
}