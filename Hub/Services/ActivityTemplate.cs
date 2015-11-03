using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
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

	            var existingWebService = uow.WebServiceRepository
		            .FindOne(x => x.Name == activityTemplateDO.WebService.Name);

				// Provided non-existing Web Service Name
	            if (existingWebService == null)
	            {
		            activityTemplateDO.WebService = null;
	            }
	            else
	            {
		            activityTemplateDO.WebService = existingWebService;
	            }

                if (!uow.ActivityTemplateRepository.GetQuery().Any(t => t.Name == activityTemplateDO.Name))
                {
                    uow.ActivityTemplateRepository.Add(activityTemplateDO);
                    uow.SaveChanges();
                }
            }
        }
    }
}
