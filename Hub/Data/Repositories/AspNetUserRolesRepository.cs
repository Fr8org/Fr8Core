using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetUserRolesRepository : GenericRepository<AspNetUserRolesDO>, IAspNetUserRolesRepository
    {

        internal AspNetUserRolesRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

        public void AssignRoleToUser(string roleName, string userID)
        {
            var roleID = GetRoleID(roleName);
            var existingRoleInDB = GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID);
            if (!DBSet.Local.Any(ur => ur.RoleId == roleID && ur.UserId == userID) && !existingRoleInDB)
                Add(new AspNetUserRolesDO { RoleId = roleID, UserId = userID });
        }
        public void RevokeRoleFromUser(string roleName, string userID)
        {
            var roleID = GetRoleID(roleName);
            var existingRoles = GetQuery().Where(ur => ur.RoleId == roleID && ur.UserId == userID).ToList();
            foreach (var existinguserRole in existingRoles)
                Remove(existinguserRole);
        }
        
        public IQueryable<AspNetRolesDO> GetRoles(string userID)
        {
            var roleIDs = GetQuery().Where(ur => ur.UserId == userID).Select(ur => ur.RoleId).ToList();
            return UnitOfWork.AspNetRolesRepository.GetQuery().Where(r => roleIDs.Contains(r.Id));
        }

        public bool UserHasRole(string roleName, string userID)
        {
            var roleID = GetRoleID(roleName);
            return GetQuery().Any(ur => ur.RoleId == roleID && ur.UserId == userID);
        }

        public String GetRoleID(String roleName)
        {
            var roleID = UnitOfWork.AspNetRolesRepository.DBSet.Local.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefault();
            if (roleID == null)
                roleID = UnitOfWork.AspNetRolesRepository.GetQuery().Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefault();
            return roleID;
        }
    }

    public interface IAspNetUserRolesRepository : IGenericRepository<AspNetUserRolesDO>
    {
        void AssignRoleToUser(string roleName, string userID);
        void RevokeRoleFromUser(string roleName, string userID);
        bool UserHasRole(string roleID, string userID); 
    }
}
