using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Hub.Interfaces
{
    public interface IActivityTemplate
    {
        IEnumerable<ActivityTemplateDO> GetAll();
        ActivityTemplateDO GetByKey(int curActivityTemplateDOId);
        void Register(ActivityTemplateDO activityTemplateDO);
       // string AssemblePluginRegistrationName(ActivityTemplateDO curActivityTemplateDO);
    }
}
