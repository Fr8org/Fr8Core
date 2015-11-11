using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hub.Interfaces;
using Hub.Managers.APIManagers;

namespace Hub.Services
{
    public class ActivityTemplate : IActivityTemplate
    {
        public IEnumerable<ActivityTemplateDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActivityTemplateRepository.GetAll();
            }
        }

        public ActivityTemplateDO GetByKey(int curActivityTemplateId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActivityTemplateDO = uow.ActivityTemplateRepository.GetByKey(curActivityTemplateId);
                if (curActivityTemplateDO == null)
                    throw new ArgumentNullException("ActionTemplateId");

                return curActivityTemplateDO;
            }

        }

        public void Register(ActivityTemplateDO activityTemplateDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var existingPlugin = uow.PluginRepository
                    .FindOne(x => x.Name == activityTemplateDO.Plugin.Name);

                if (existingPlugin != null)
                {
                    activityTemplateDO.Plugin = existingPlugin;
                }
                else
                {
                    uow.PluginRepository.Add(activityTemplateDO.Plugin);
                    uow.SaveChanges();
                }

	            if (activityTemplateDO.WebService != null)
	            {
					var existingWebService = uow.WebServiceRepository.FindOne(x => x.Name == activityTemplateDO.WebService.Name);

		            if (existingWebService != null)
		            {
			            activityTemplateDO.WebService = existingWebService;
		            }
		            else
		            {
						activityTemplateDO.WebService = null;
		            }
	            }

                var activity = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(t => t.Name == activityTemplateDO.Name);

                if (activity == null)
                {
                    uow.ActivityTemplateRepository.Add(activityTemplateDO);
                }
                else
                {
                    activity.ActivityTemplateState = ActivityTemplateState.Active;
                }
                uow.SaveChanges();
            }
        }
    }
}
