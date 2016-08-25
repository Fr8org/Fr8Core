using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Infrastructure;

namespace Hub.Interfaces
{
    public interface IActivityTemplate
    {
        ActivityTemplateInfo GetActivityTemplateInfo(string fullActivityTemplateName);

        IEnumerable<ActivityTemplateDO> GetQuery();
        ActivityTemplateDO[] GetAll();
        ActivityTemplateDO GetByKey(Guid curActivityTemplateDOId);
        bool TryGetByKey(Guid activityTemplateId, out ActivityTemplateDO activityTemplate);
        string GetTerminalUrl(Guid? curActivityTemplateDOId);
        void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo);
        ActivityTemplateDO GetByNameAndVersion(ActivityTemplateSummaryDTO activityTemplateSummary);
        void RemoveInactiveActivities(TerminalDO terminal, List<ActivityTemplateDO> activityTemplateDO);
        bool Exists(Guid activityTemplateId);
    }
}
