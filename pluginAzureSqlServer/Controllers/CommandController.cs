using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace pluginAzureSqlServer.Controllers
{
    public class CommandController : ApiController
    {
        [HttpPost]
        [Route("writeSQL")]
        public void Write(JObject data)
        {

        }
    }
}
