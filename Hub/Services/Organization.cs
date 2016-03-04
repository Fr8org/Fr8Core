using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;

namespace Hub.Services
{
    public class Organization : IOrganization
    {
        /// <summary>
        /// Check if already exists some organization with the same name and create new if not
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="organizationName"></param>
        /// <returns></returns>
        public OrganizationDO GetOrCreateOrganization(IUnitOfWork uow, string organizationName)
        {
            OrganizationDO organization = null;
            //check the organization if already exist
            if (!string.IsNullOrEmpty(organizationName))
            {
                organization = uow.OrganizationRepository.GetQuery().FirstOrDefault(x => x.Name == organizationName);
                if (organization == null)
                {
                    //create new organization
                    organization = new OrganizationDO()
                    {
                        Name = organizationName
                    };
                    uow.OrganizationRepository.Add(organization);
                }
            }

            return organization;
        }
    }
}
