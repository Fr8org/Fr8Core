using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    public interface ISecurityObjectsCache
    {
        ObjectRolePrivilegesDO Get(string id);
        void AddOrUpdate(string id, ObjectRolePrivilegesDO rolePrivileges);
    }
}
