using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Infrastructure.StructureMap;
using Data.Repositories.MultiTenant;
using Data.Repositories.Security.Entities;
using Data.Repositories.SqlBased;
using Data.States;
using StructureMap;

namespace Data.Repositories.Security.StorageImpl
{
    class InMemorySecurityObjectsStorageProvider : ISecurityObjectsStorageProvider
    {
        private static readonly List<ObjectRolePrivilegesDO> ObjectRolePrivileges = new List<ObjectRolePrivilegesDO>();
        private string roleName;
        private Guid readRolePrivilegeId;
        private Guid editRolePrivilegeID;

        public InMemorySecurityObjectsStorageProvider(ISqlConnectionProvider sqlConnectionProvider)
        {
            var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            roleName  = securityServices.GetRoleNames().FirstOrDefault();
            readRolePrivilegeId = Guid.Parse("01ee8bb1-05d0-41fd-bd55-6fcd69ec5ec7");
            editRolePrivilegeID = Guid.Parse("7cb466dc-8fed-4791-a1ba-09f9135416db");
        }

        public List<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId)
        {
            throw new NotImplementedException();
        }

        public ObjectRolePrivilegesDO GetRolePrivilegesForSecuredObject(string dataObjectId)
        {
            lock (ObjectRolePrivileges)
            {
                return ObjectRolePrivileges.FirstOrDefault(x => x.ObjectId == dataObjectId);
            }
        }

        public int InsertObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string dataObjectType,
            string propertyName = null)
        {
            lock (ObjectRolePrivileges)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    ObjectRolePrivileges.Add(new ObjectRolePrivilegesDO() { ObjectId = dataObjectId,
                        RolePrivileges = new List<RolePrivilege>() { new RolePrivilege { Id = rolePrivilegeId,
                            Privilege = new PrivilegeDO() {Name = Privilege.ReadObject.ToString() }
                        ,Role = new RoleDO() { RoleName = roleName, } } } });
                }
                else
                {
                    var objectRolePrivileges = new ObjectRolePrivilegesDO()
                    {
                        ObjectId = dataObjectId,
                        Properties = new Dictionary<string, List<RolePrivilege>>()
                    };
                    objectRolePrivileges.Properties.Add(propertyName, new List<RolePrivilege>() { new RolePrivilege { Id = rolePrivilegeId,
                        Privilege = new PrivilegeDO() {Name = Privilege.ReadObject.ToString() },
                        Role = new RoleDO() { RoleName = roleName } } });
                }
                    
                return 1;
            }
        }

        public int InsertRolePrivilege(RolePrivilege rolePrivilege)
        {
            throw new NotImplementedException();
        }

        public int RemoveObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string propertyName = null)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType)
        {
            lock (ObjectRolePrivileges)
            {
                var objectRolePrivilege = new ObjectRolePrivilegesDO()
                {
                    ObjectId = dataObjectId,
                    RolePrivileges = new List<RolePrivilege>()
                    {
                        new RolePrivilege()
                        {
                            Privilege = new PrivilegeDO() { Name = Privilege.ReadObject.ToString()},
                            Role = new RoleDO()
                            {
                                RoleName = roleName,
                            }
                        },
                        new RolePrivilege()
                        {
                            Privilege = new PrivilegeDO() { Name = Privilege.EditObject.ToString()},
                            Role = new RoleDO()
                            {
                                RoleName = roleName
                            } 
                        },
                        new RolePrivilege()
                        {
                            Privilege = new PrivilegeDO() { Name = Privilege.DeleteObject.ToString()},
                            Role = new RoleDO()
                            {
                                RoleName = roleName
                            }
                        }
                    }
                };
                ObjectRolePrivileges.Add(objectRolePrivilege);
            }
        }

        public int UpdateRolePrivilege(RolePrivilege rolePrivilege)
        {
            throw new NotImplementedException();
        }
    }
}
