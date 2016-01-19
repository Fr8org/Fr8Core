using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Interfaces.DataTransferObjects;
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
                var existingTerminal = uow.TerminalRepository
                    .FindOne(x => x.Name == activityTemplateDO.Terminal.Name);

                if (existingTerminal != null)
                {
                    activityTemplateDO.Terminal = existingTerminal;
                }
                else
                {
                    uow.TerminalRepository.Add(activityTemplateDO.Terminal);
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
                        //Add a new Web service
                        if (activityTemplateDO.WebService != null)
                        {
                            uow.Db.Entry(activityTemplateDO.WebService).State = System.Data.Entity.EntityState.Added;
                        }
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

        /// <summary>
        /// Returns ActivityTemplate by it's name.
        /// For example GetByName(uow, "AddPayloadManually_v1").
        /// </summary>
        public ActivityTemplateDO GetByName(IUnitOfWork uow, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ApplicationException("Invalid ActivityTemplate name");
            }

            var tokens = name.Split('_');
            if (tokens.Length < 2)
            {
                throw new ApplicationException("Invalid ActivityTemplate name");
            }

            var versionToken = tokens[tokens.Length - 1];

            if (versionToken == null || versionToken.Length < 2)
            {
                throw new ApplicationException("Invalid ActivityTemplate name");
            }

            var namePart = string.Join("_", tokens.Take(tokens.Length - 1).ToArray());
            var versionPart = versionToken.Substring(1);

            return GetByNameAndVersion(uow, namePart, versionPart);
        }

        /// <summary>
        /// Returns ActivityTemplate by it's name and version.
        /// For example GetByNameAndVersion(uow, "AddPayloadManually", "1").
        /// </summary>
        public ActivityTemplateDO GetByNameAndVersion(IUnitOfWork uow, string name, string version)
        {
            var activityTemplate = uow.ActivityTemplateRepository
                .FindOne(x => x.Name == name && x.Version == version);

            return activityTemplate;
        }
    }
}
