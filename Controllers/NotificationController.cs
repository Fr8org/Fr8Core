using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using StructureMap;
using Utilities.Logging;

namespace Web.Controllers
{
    public class NotificationController : ApiController
    {
        IProcessService _processService;
        AlertReporter _alertReporter;

        public NotificationController()
        {
            _processService = ObjectFactory.GetInstance<IProcessService>();
            _alertReporter = ObjectFactory.GetInstance<AlertReporter>();
        }

        public NotificationController(IProcessService processService)
        {
            _processService = processService;
            _alertReporter = ObjectFactory.GetInstance<AlertReporter>();
        }
        /// <summary>
        /// Processes incoming DocuSign notifications.
        /// </summary>
        /// <returns>HTTP 200 if notification is successfully processed, 
        /// HTTP 400 if request does not contain all expected data or malformed.</returns>
        [HttpPost]
        public async Task<IHttpActionResult> HandleDocusignNotification([FromUri] string userId)
        {
            var xmlPayload = await this.Request.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(userId))
            {
                string message = "Cannot find userId in DocuSign notification. XML payload";
                _alertReporter.ImproperDocusignNotificationReceived(message);
                return BadRequest(message);
            }

            if (string.IsNullOrEmpty(xmlPayload))
            {
                string message = String.Format("Cannot find XML payload in DocuSign notification: UserId {0}.",
                    userId);
                _alertReporter.ImproperDocusignNotificationReceived(message);
                return BadRequest(message);
            }

            try
            {
                _processService.HandleDocusignNotification(userId, xmlPayload);
            }
            catch (ArgumentException)
            {
                //The event is already logged.
                return BadRequest("Cannot find envelopeId in XML payload.");
            }
            return Ok();               
        }
    }
}