using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Web.Controllers
{
    [RoutePrefix("field")]
    public class FieldController : ApiController
    {
        private readonly IField _field;

        public FieldController()
        {
            _field = ObjectFactory.GetInstance<IField>();
        }

        public FieldController(IField service)
        {
            _field = service;
        }


        [HttpPost]
        //[Fr8ApiAuthorize]
        [Route("exists")]
        //[ResponseType(typeof(ResponseType))]
        public async Task<IHttpActionResult> Exists(List<FieldCheckDTO> fieldCheckList)
        {
            //create a response type
            String testResult = "";
            foreach (var fieldCheck in fieldCheckList)
            {
                //build response type
                testResult += _field.Exists(fieldCheck);
                testResult += ",";
            }

            //and return this Responsetype as list

            return Ok(testResult);
        }    
    }
}