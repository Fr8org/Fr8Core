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
        [HttpGet]
        public IHttpActionResult Get(string OrganizationName, bool isNew)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                
                OrganizationDO organization = _organization.GetOrCreateOrganization(uow, OrganizationName,out isNew);
                return Ok(organization);
            }
        }
        /// <summary>
        /// Recieve criteria with global id, update criteria,
        /// and return updated criteria.
        /// </summary>
        /// <param name="dto">Criteria data transfer object.</param>
        /// <returns>Updated criteria.</returns>
        [ResponseType(typeof(OrganizationDTO))]
        [HttpPost]
        public IHttpActionResult Post(OrganizationDTO dto)
        {
            Mapper.CreateMap<OrganizationDTO, OrganizationDO>();
            OrganizationDO entity = Mapper.Map<OrganizationDO>(dto);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.OrganizationRepository.Add(entity);

                uow.SaveChanges();
            }
            Mapper.CreateMap<OrganizationDO, OrganizationDTO>();
            var model = Mapper.Map<OrganizationDTO>(entity);
            return Ok(model);
            
        }
        [ResponseType(typeof(OrganizationDTO))]
        [HttpPut]
        public IHttpActionResult Update(OrganizationDTO dto)
        {

            OrganizationDO curOrganization = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curOrganization = uow.OrganizationRepository.GetByKey(dto.Id);
                if (curOrganization == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", dto.Id));
                }

                Mapper.Map<OrganizationDTO, OrganizationDO>(dto, curOrganization);

                uow.SaveChanges();
            }
            return Ok(Mapper.Map<OrganizationDTO>(curOrganization));
        }
    }
}