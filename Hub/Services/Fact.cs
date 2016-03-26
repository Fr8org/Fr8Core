using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace Hub.Services
{
    public class Fact : Hub.Interfaces.IFact
    {
        public IList<FactDO> GetByObjectId(IUnitOfWork unitOfWork, string id)
        {
            var query = unitOfWork.FactRepository.GetQuery();
            return query.Where(fact => fact.ObjectId == id).ToList();
        }

        // Currently GetAll returns facts only for Admin Role
        public IList<FactDO> GetAll(IUnitOfWork unitOfWork, ICollection<IdentityUserRole> roles = null)
        {
            IList<FactDO> facts = new List<FactDO>();
            if (roles != null)
            {
                //get the role id
                var adminRoleId = unitOfWork.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
                //provide all facts if the user has admin role
                if (roles.Any(x => x.RoleId == adminRoleId))
                {
                    facts = unitOfWork.FactRepository.GetQuery()
                    .OrderByDescending(i => i.CreateDate)
                    .Take(200)
                    .ToList();
                }
            }
            return facts;
        }
    }
}
