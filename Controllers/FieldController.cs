using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Constants;
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
        [ResponseType(typeof(List<FieldCheckResult>))]
        public async Task<IHttpActionResult> Exists(List<FieldCheckDTO> fieldCheckList)
        {
            var result = new List<FieldCheckResult>();
            foreach (var fieldCheck in fieldCheckList)
            {
                result.Add(_field.Exists(fieldCheck) ? FieldCheckResult.Exists : FieldCheckResult.NotExists);
            }
            return Ok(result);
        }    
    }
}