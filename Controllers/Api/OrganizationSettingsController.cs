using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.ViewModels;
using System;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    public class OrganizationSettingsController: ApiController
	{
        private readonly IOrganization _organization;

        public OrganizationSettingsController()
        {
            _organization = ObjectFactory.GetInstance<IOrganization>();
        }

        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            return Ok(_organization.GetOrganizationById(id));
        }
        
        
        [ResponseType(typeof(OrganizationDTO))]
        [HttpPut]
        public IHttpActionResult Put(OrganizationDTO dto)
        {
            return Ok(_organization.UpdateOrganization(dto));
        }
    }
}