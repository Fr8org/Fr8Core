using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Newtonsoft.Json.Linq;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Web.WebApiControllers
{
    public class UserEmailsApiController : ApiController
    {
        
        // GET api/<controller>
        public HttpResponseMessage Get()
        {
           // var response = 
            var data = new UserEmails().GetUserEmails();
            var list = (from model in data
                        select new { 
                        Id=model.Id,
                        Address=model.Address
                        });
          return new HttpResponseMessage()
          {
              Content = new StringContent(JArray.FromObject(list).ToString(), Encoding.UTF8, "application/json")
          };
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