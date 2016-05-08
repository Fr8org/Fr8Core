using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.MultiTenant;
using Data.Repositories.Security.Entities;
using Data.Repositories.SqlBased;
using Data.States;
using StructureMap;

namespace Data.Repositories.Security.StorageImpl
{
    class InMemorySecurityObjectsStorageProvider : ISecurityObjectsStorageProvider
    {
        private static readonly List<ObjectRolePermissionsWrapper> ObjectRolePermissions = new List<ObjectRolePermissionsWrapper>();
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

        public int InsertObjectRolePermission(string dataObjectId, Guid rolePrivilegeId, string dataObjectType,
            string propertyName = null)
        {
            lock (ObjectRolePermissions)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    //ObjectRolePermissions.Add(new ObjectRolePermissionsWrapper() { ObjectId = dataObjectId,
                    //    RolePermissions = new List<RolePermission>() { new RolePermission { Id = rolePrivilegeId,
                    //        PermissionSet = new PermissionSetDO() {}
                    //    ,Role = new RoleDO() { RoleName = roleName, } } } });
                }
                else
                {
                    //var objectRolePermissions = new ObjectRolePermissionsDO()
                    //{
                    //    ObjectId = dataObjectId,
                    //    Properties = new Dictionary<string, List<RolePermission>>()
                    //};
                    //objectRolePermissions.Properties.Add(propertyName, new List<RolePermission>() { new RolePermission { Id = rolePrivilegeId,
                    //        PermissionSet = new PermissionSetDO() {},
                    //    Role = new RoleDO() { RoleName = roleName } } });
                }
                    
                return 1;
            }
        }

        public int InsertRolePermission(RolePermission rolePrivilege)
        {
            throw new NotImplementedException();
        }

        public int RemoveObjectRolePermission(string dataObjectId, Guid rolePrivilegeId, string propertyName = null)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType)
        {
            lock (ObjectRolePermissions)
            {
                //var objectRolePrivilege = new ObjectRolePermissionsDO()
                //{
                //    ObjectId = dataObjectId,
                //    RolePermissions = new List<RolePermission>()
                //    {
                //        new RolePermission()
                //        {
                //            PermissionSet = new PermissionSetDO() {},
                //            Role = new RoleDO()
                //            {
                //                RoleName = roleName,
                //            }
                //        },
                //        new RolePermission()
                //        {
                //            PermissionSet = new PermissionSetDO() {},
                //            Role = new RoleDO()
                //            {
                //                RoleName = roleName
                //            } 
                //        },
                //        new RolePermission()
                //        {
                //            PermissionSet = new PermissionSetDO() {},
                //            Role = new RoleDO()
                //            {
                //                RoleName = roleName
                //            }
                //        }
                //    }
                //};
                //ObjectRolePermissions.Add(objectRolePrivilege);
            }
        }

        public int UpdateRolePermission(RolePermission rolePermissions)
        {
            throw new NotImplementedException();
        }

        public List<PermissionDTO> GetAllPermissionsForUser(Guid profileId)
        {
            throw new NotImplementedException();
        }

        public List<int> GetObjectBasedPermissionSetForObject(string dataObjectId, string dataObjectType, Guid profileId)
        {
            return new List<int>();
        }

        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(string dataObjectId)
        {
            lock (ObjectRolePermissions)
            {
                return ObjectRolePermissions.FirstOrDefault(x => x.ObjectId == dataObjectId);
            }
        }
    }
}
