using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Security;
using Data.Repositories.Security.Entities;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;

namespace Hub.Services
{
    public class Organization : IOrganization
    {
        public static string MemberOfOrganizationRoleName(string organizationName)
        {
            return $"MemberOfOrganization_{organizationName}";
        }

        public static string AdminOfOrganizationRoleName(string organizationName)
        {
            return $"AdminOfOrganization_{organizationName}";
        }

        public OrganizationDTO GetOrganizationById(int id) {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var organization = uow.OrganizationRepository.GetByKey(id);
                var model = Mapper.Map<OrganizationDTO>(organization);
                return model;
            }
        }

        public OrganizationDTO UpdateOrganization(OrganizationDTO dto) {
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
            return Mapper.Map<OrganizationDTO>(curOrganization);
        }

        /// <summary>
        /// Check if already exists some organization with the same name and create new if not
        /// </summary>
        /// <param name="organizationName"></param>
        /// <param name="isNewOrganization"></param>
        /// <returns></returns>
        public OrganizationDO GetOrCreateOrganization(IUnitOfWork uow, string organizationName, out bool isNewOrganization)
        {
            OrganizationDO organization = null;
            isNewOrganization = false;
            //check if organization already exist
            organization = uow.OrganizationRepository.GetQuery().FirstOrDefault(x => x.Name == organizationName);

            if (organization != null) return organization;
                
            //create new organization
            organization = new OrganizationDO()
            {
                Name = organizationName
            };
            uow.OrganizationRepository.Add(organization);

            //create roles for new organization
            uow.AspNetRolesRepository.Add(new AspNetRolesDO()
            {
                Name = MemberOfOrganizationRoleName(organizationName)
            });

            var adminRole = new AspNetRolesDO()
            {
                Name = AdminOfOrganizationRoleName(organizationName)
            };
            uow.AspNetRolesRepository.Add(adminRole);

            isNewOrganization = true;
                
            uow.SaveChanges();

            //link adminRole with ManageInternalUsers Privilege, used for  add/edit users that belong to this organization
            var securityObjectStorage = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            securityObjectStorage.InsertRolePrivilege(new RolePrivilege()
                {
                    Privilege = new PrivilegeDO() { Name = Privilege.ManageInternalUsers.ToString()},
                    Role = new RoleDO() { RoleId = adminRole.Id, }
                });

            return organization;
        }
    }
}
