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
        /// <returns></returns>
        OrganizationDO GetOrCreateOrganization(IUnitOfWork uow, string organizationName);
    }
}
