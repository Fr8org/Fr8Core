using System;
using Data.Entities;
using Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure.StructureMap;

namespace Hub.Services
{
    public class Fact : Hub.Interfaces.IFact
    {
        private readonly ISecurityServices _securityServices;

        public Fact(ISecurityServices security)
        {
            _securityServices = security;
        }

        public IList<FactDO> GetByObjectId(IUnitOfWork unitOfWork, string id)
        {
            //here we should determine type of ObjectId, to look at it`s owner in appropriate table
            //but it is non trivial action so for now select facts where userid is the same as fr8UserId

            if (_securityServices.IsCurrentUserHasRole(Data.States.Roles.Admin))
            {
                return
                    unitOfWork.FactRepository.GetQuery().Where(
                        fact =>
                            fact.ObjectId == id
                            ).ToList();
            }
            else
            {
                var fr8UserId = _securityServices.GetCurrentAccount(unitOfWork).Id;
                return
                    unitOfWork.FactRepository.GetQuery().Where(
                        fact =>
                            fact.ObjectId == id &&
                            fact.Fr8UserId.Equals(fr8UserId, StringComparison.InvariantCultureIgnoreCase))
                            .ToList();
            }
            
        }

        //@tony.yakovets: i haven`t found any usages of  GetAll, besides i uncoment Fact controller, so i think i can comment out this method 

        // Currently GetAll returns facts only for Admin Role
        //public IList<FactDO> GetAll(IUnitOfWork unitOfWork, ICollection<IdentityUserRole> roles = null)
        //{
        //    IList<FactDO> facts = new List<FactDO>();
        //    if (roles != null)
        //    {
        //        //get the role id
        //        var adminRoleId = unitOfWork.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
        //        //provide all facts if the user has admin role
        //        if (roles.Any(x => x.RoleId == adminRoleId))
        //        {
        //            facts = unitOfWork.FactRepository.GetQuery()
        //            .OrderByDescending(i => i.CreateDate)
        //            .Take(200)
        //            .ToList();
        //        }
        //    }
        //    return facts;
        //}
    }
}
