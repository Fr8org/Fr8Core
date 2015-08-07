using System;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using pluginAzureSqlServer.Infrastructure;
using pluginAzureSqlServer.Messages;
using pluginAzureSqlServer.Services;

namespace pluginAzureSqlServer.Controllers
{
    public class ActionController : ApiController
    {
        public const string Version = "1.0";

        /// <summary>
        /// Insert user data to remote database tables.
        /// </summary>
        [HttpPost]
        [Route("writeSQL")]
        public CommandResponse Write(JObject data)
        {
            try
            {
                // Creating ExtrationHelper and parsing WriteCommandArgs.
                var parser = new DbServiceJsonParser();
                var writeArgs = parser.ExtractWriteCommandArgs(data);

                // Creating DbService and running WriteCommand logic.
                var dbService = new DbService();
                dbService.WriteCommand(writeArgs);
            }
            catch (Exception ex)
            {
                return CommandResponse.ErrorResponse(ex.Message);
            }

            return CommandResponse.SuccessResponse();
        }
    }
}