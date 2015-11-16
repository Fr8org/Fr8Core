using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IActivityTemplate
    {
        IEnumerable<ActivityTemplateDO> GetAll();
        ActivityTemplateDO GetByKey(int curActivityTemplateDOId);
        void Register(ActivityTemplateDO activityTemplateDO);
        ActivityTemplateDO GetByName(IUnitOfWork uow, string name);
        ActivityTemplateDO GetByNameAndVersion(IUnitOfWork uow, string name, string version);
       // string AssemblePluginRegistrationName(ActivityTemplateDO curActivityTemplateDO);
    }
}
