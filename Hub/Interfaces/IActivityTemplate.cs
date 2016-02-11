using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IActivityTemplate
    {
        IEnumerable<ActivityTemplateDO> GetQuery();
        ActivityTemplateDO[] GetAll();
        ActivityTemplateDO GetByKey(int curActivityTemplateDOId);
        string GetTerminalUrl(int? curActivityTemplateDOId);
        void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo);
        ActivityTemplateDO GetByName(IUnitOfWork uow, string name);
        ActivityTemplateDO GetByNameAndVersion(IUnitOfWork uow, string name, string version);
       // string AssemblePluginRegistrationName(ActivityTemplateDO curActivityTemplateDO);
    }
}
