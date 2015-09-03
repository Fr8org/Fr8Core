using Core.Services;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Web.WebApiControllers
{
    public class UserEmailsApiController : ApiController
    {
        
        // GET api/<controller>
        public IEnumerable<EmailAddressDO> Get()
        {
           // var response = 
            return new UserEmails().GetUserEmails();
           // return Request.CreateResponse(HttpStatusCode.OK, response, Configuration.Formatters.JsonFormatter);
         // return new HttpResponseMessage(){Content=new JsonContent { new UserEmails().GetUserEmails()};
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}