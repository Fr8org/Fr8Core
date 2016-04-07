using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Security;
using Data.Repositories.Security.Entities;
using Data.States;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class Organization : IOrganization
    {
        public static string MemberOfOrganizationRoleName(string organizationName)
        {
            return $"MemberOfOrganization_{organizationName}";
        }

        public static string AdminfOrganizationRoleName(string organizationName)
        {
            return $"AdminOfOrganization_{organizationName}";
        }

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
                    Name = MemberOfOrganizationRoleName(organizationName)
                });

                var adminRole = new AspNetRolesDO()
                {
                    Name = AdminfOrganizationRoleName(organizationName)
                };
                uow.AspNetRolesRepository.Add(adminRole);

                isNewOrganization = true;
                
                uow.SaveChanges();

                //link adminRole with ManageInternalUsers Privilege, used for  add/edit users that belong to this organization
                var securityObjectStorage = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
                securityObjectStorage.InsertRolePrivilege(new RolePrivilege() { RoleId = adminRole.Id, PrivilegeName = Privilege.ManageInternalUsers.ToString()});

                return organization;
            }
        }
    }
}
