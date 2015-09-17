using Data.Entities;
using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
    public class ComponentActivitiesDTO
    {
        public List<ActivityTemplateDO> ComponentActivities { get; set; }
    }
}
