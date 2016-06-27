using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Hub.Infrastructure;

namespace Hub.Interfaces
{
    public interface IActivityTemplate
    {
        ActivityTemplateInfo GetActivityTemplateInfo(string fullActivityTemplateName);

        IEnumerable<ActivityTemplateDO> GetQuery();
        ActivityTemplateDO[] GetAll();
        ActivityTemplateDO GetByKey(Guid curActivityTemplateDOId);
        //ActivityTemplateDO GetByActivityKey(Guid curActivityId);
        string GetTerminalUrl(Guid? curActivityTemplateDOId);
        void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo);
        ActivityTemplateDO GetByName(IUnitOfWork uow, string name);
        ActivityTemplateDO GetByNameAndVersion(string name, string version);
        // string AssemblePluginRegistrationName(ActivityTemplateDO curActivityTemplateDO);
        void RemoveInactiveActivities(List<ActivityTemplateDO> activityTemplateDO);
    }
}
