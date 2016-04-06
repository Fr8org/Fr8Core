using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IOrganization
    {
        /// <summary>
        /// Check if already exists some organization with the same name and create new if not
        /// </summary>
        /// <param name="organizationName"></param>
        /// <param name="isNewOrganization"></param>
        /// <returns></returns>
        OrganizationDO GetOrCreateOrganization(string organizationName, out bool isNewOrganization);
    }
}
