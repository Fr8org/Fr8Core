using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IActivityTemplate
    {
        IEnumerable<ActivityTemplateDO> GetAll();
        ActivityTemplateDO GetByKey(int curActivityTemplateDOId);
       // string AssemblePluginRegistrationName(ActivityTemplateDO curActivityTemplateDO);
    }
}
