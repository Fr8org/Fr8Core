using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface IPlanDescription
    {
        PlanDescriptionDO Save(Guid planId, string curFr8UserId);
        List<PlanDescriptionDO> GetDescriptions(string userId);
    }
}
