using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    public interface ISecurityObjectsCache
    {
        ObjectRolePermissionsWrapper GetRecordBasedPermissionSet(string id);
        List<PermissionSetDO> GetProfilePermissionSets(string id);
        void AddOrUpdateRecordBasedPermissionSet(string id, ObjectRolePermissionsWrapper rolePermissions);
        void AddOrUpdateProfile(string id, List<PermissionSetDO> rolePermissions);
    }
}
