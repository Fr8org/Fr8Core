using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Data.Constants;
using HubWeb.Infrastructure;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Services;

namespace HubWeb.Controllers
{
    //[RoutePrefix("field")]
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
        [ResponseType(typeof(List<FieldValidationResult>))]
        [Fr8HubWebHMACAuthorize]
        public async Task<IHttpActionResult> Exists(List<FieldValidationDTO> fieldCheckList)
        {
            var result = new List<FieldValidationResult>();
            foreach (var fieldCheck in fieldCheckList)
            {
                result.Add(_field.Exists(fieldCheck) ? FieldValidationResult.Exists : FieldValidationResult.NotExists);
            }
            return Ok(result);
        }    
    }
}