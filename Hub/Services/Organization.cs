using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Security;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class Organization : IOrganization
    {
        /// <summary>
        /// Check if already exists some organization with the same name and create new if not
        /// </summary>
        /// <param name="organizationName"></param>
        /// <param name="isNewOrganization"></param>
        /// <returns></returns>
        public OrganizationDO GetOrCreateOrganization(string organizationName, out bool isNewOrganization)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
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
                    Name = $"MemberOfOrganization_{organizationName}"
                });

                var adminRole = new AspNetRolesDO()
                {
                    Name = $"AdminOfOrganization_{organizationName}"
                };
                uow.AspNetRolesRepository.Add(adminRole);

                isNewOrganization = true;
                
                uow.SaveChanges();

                //link adminRole with user Privilege
                var securityObjectStorage = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
                securityObjectStorage

                return organization;
            }
        }
    }
}
