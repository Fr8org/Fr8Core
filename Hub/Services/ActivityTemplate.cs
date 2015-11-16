﻿using Data.Entities;
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
    }
}
