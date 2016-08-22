using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using StructureMap;
using terminalFacebook.Interfaces;
using terminalFacebook.Models;

namespace terminalFacebook.Controllers
{
    [RoutePrefix("terminals/terminalFacebook")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly IHubEventReporter _reporter;
        private readonly IContainer _container;
        
        public EventController(IEvent @event, IHubEventReporter reporter, IContainer container)
        {
            _event = @event;
            _reporter = reporter;
            _container = container;
        }

        private static byte[] SignWithHmac(byte[] dataToSign, byte[] keyBody)
        {
            using (var hmacAlgorithm = new HMACSHA1(keyBody))
            {
                return hmacAlgorithm.ComputeHash(dataToSign);
            }
        }

        private static string ConvertToHexadecimal(IEnumerable<byte> bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        private static bool IsHashValid(string hash, string data)
        {
            var secret = CloudConfigurationManager.GetSetting("FacebookSecret");
            var hmac = SignWithHmac(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(secret));
            var computedHash = ConvertToHexadecimal(hmac);
            return computedHash == hash;           
        }

        [HttpPost]
        [Route("usernotifications")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            //lets verify request is made from facebook
            var hash = Request.Headers.GetValues("x-hub-signature").FirstOrDefault();
            string eventPayLoadContent = await Request.Content.ReadAsStringAsync();
            //first 5 characters of hash is "sha1="
            //therefore we are removing it
            if (hash == null || hash.Length < 6 || !IsHashValid(hash.Substring(5), eventPayLoadContent))
            {
                return NotFound();
            }
            Debug.WriteLine($"Processing event request for fb: {eventPayLoadContent}");
            var eventList = await _event.ProcessUserEvents(_container, eventPayLoadContent);
            foreach (var fbEvent in eventList)
            {
                await _reporter.Broadcast(fbEvent);
            }
            return Ok("Processed Facebook event notification successfully.");
        }

        
        [HttpGet]
        [Route("usernotifications")]
        public IHttpActionResult VerifyFacebookWebhookRegistration(VerificationMessage msg)
        {
            if (msg.VerifyToken == "fr8facebookeventverification")
            {
                return Ok(msg.Challenge);
            }
            return Ok("Unknown verification call");
        }
    }

    
}
