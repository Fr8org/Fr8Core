using System;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IAspNetRolesDO : IBaseDO
    {
        String Id { get; set; }
        String Name { get; set; }
    }
}
        
