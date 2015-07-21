using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using StructureMap;
using Utilities.Logging;

namespace Web.Controllers
{
    public class NotificationController : ApiController
    {
        IProcess _processService;

        public NotificationController()
        {
            _processService = ObjectFactory.GetInstance<IProcess>();
        }

        /// <summary>
        /// This constructor is intended for unit testing since it allows injecting 
        /// a dynamically mocked service.
        /// </summary>
        public NotificationController(IProcess processService)
        {
            _processService = processService;
        }
        /// <summary>
        /// Processes incoming DocuSign notifications.
        /// </summary>
        /// <returns>HTTP 200 if notification is successfully processed, 
        /// HTTP 401 if request does not contain all expected data or malformed.</returns>
        [HttpPost]
        public async Task<IHttpActionResult> HandleDocusignNotification([FromUri] string userId)
        {
            var xmlPayload = await this.Request.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(userId))
            {
                string message = String.Format("Cannot userId in DocuSign notification.");
                Logger.GetLogger().Warn(message);
                return BadRequest(message);
            }

            if (string.IsNullOrEmpty(xmlPayload))
            {
                string message = String.Format("Cannot find XML payload in DocuSign notification: UserId {0}.",
                    userId);
                Logger.GetLogger().Warn(message);
                return BadRequest(message);
            }

            try
            {
                _processService.HandleDocusignNotification(userId, xmlPayload);
            }
            catch (InvalidOperationException)
            {
                //The event is already logged.
                return BadRequest("Cannot find envelopeId in XML payload.");
            }
            return Ok();               
        }
    }
}