using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper.Internal;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class ActivityTemplate : IActivityTemplate
    {
        private readonly ITerminal _terminal;
        private readonly Dictionary<int, ActivityTemplateDO> _activityTemplates = new Dictionary<int, ActivityTemplateDO>();
        private bool _isInitialized;
        
        public ActivityTemplate(ITerminal terminal)
        {
            _terminal = terminal;
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (_activityTemplates)
            {
                if (_isInitialized)
                {
                    return;
                }

                LoadFromDb();

                _isInitialized = true;
            }
        }

        private void LoadFromDb()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var activityTemplate in uow.ActivityTemplateRepository.GetQuery().Include(x=>x.WebService))
                {
                    _activityTemplates[activityTemplate.Id] = Clone(activityTemplate);
                }
            }
        }

        public ActivityTemplateDO[] GetAll()
        {
            Initialize();

            lock (_activityTemplates)
            {
                return _activityTemplates.Values.ToArray();
            }
        }

        public string GetTerminalUrl(int? curActivityTemplateId)
        {
            if (curActivityTemplateId == null)
            {
                return null;
            }

            Initialize();

            return GetByKey(curActivityTemplateId.Value).Terminal.Endpoint;
        }

        public ActivityTemplateDO GetByKey(int curActivityTemplateId)
        {
            Initialize();

            lock (_activityTemplates)
            {
                ActivityTemplateDO template;

                if (!_activityTemplates.TryGetValue(curActivityTemplateId, out template))
                {
                    throw new KeyNotFoundException(string.Format("Can't find activity template with id {0}", curActivityTemplateId));
                }

                return template;
            }
        }

        public IEnumerable<ActivityTemplateDO> GetQuery()
        {
            Initialize();

            lock (_activityTemplates)
            {
                return _activityTemplates.Values.ToArray();
            }
        }

        private ActivityTemplateDO Clone(ActivityTemplateDO source)
        {
            var newTemplate = new ActivityTemplateDO();

            newTemplate.ActivityTemplateState = source.ActivityTemplateState;
            newTemplate.Category = source.Category;
            newTemplate.ComponentActivities = source.ComponentActivities;
            newTemplate.Description = source.Description;
            newTemplate.Id = source.Id;
            newTemplate.Label = source.Label;
            newTemplate.MinPaneWidth = source.MinPaneWidth;
            newTemplate.Name = source.Name;
            newTemplate.NeedsAuthentication = source.NeedsAuthentication;
            newTemplate.Tags = source.Tags;
            newTemplate.TerminalId = source.TerminalId;
            newTemplate.Type = source.Type;
            newTemplate.Version = source.Version;
            newTemplate.Terminal = _terminal.GetByKey(source.TerminalId);
          
            if (source.WebService != null)
            {
                newTemplate.WebService = new WebServiceDO
                {
                    IconPath = source.WebService.IconPath,
                    Name = source.WebService.Name,
                    Id = source.WebService.Id
                };
            }

            newTemplate.WebServiceId = source.WebServiceId;

            return newTemplate;
        }

        public void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo)
        {
            if (activityTemplateDo == null)
            {
                return;
            }

            _terminal.RegisterOrUpdate(activityTemplateDo.Terminal);

            Initialize();

            lock (_activityTemplates)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    if (activityTemplateDo.WebService != null)
                    {
                        var existingWebService = uow.WebServiceRepository.FindOne(x => x.Name == activityTemplateDo.WebService.Name);

                        if (existingWebService != null)
                        {
                            activityTemplateDo.WebService = existingWebService;
                        }
                        else
                        {
                            //Add a new Web service
                            if (activityTemplateDo.WebService != null)
                            {
                                uow.Db.Entry(activityTemplateDo.WebService).State = EntityState.Added;
                            }
                        }
                    }
                    
                    var activity = uow.ActivityTemplateRepository.GetQuery().FirstOrDefault(t => t.Name == activityTemplateDo.Name);

                    if (activity == null)
                    {
                        uow.ActivityTemplateRepository.Add(activity = activityTemplateDo);
                        uow.SaveChanges();
                    }
                    else
                    {
                        activity.ActivityTemplateState = ActivityTemplateState.Active;
                        activity.Category = activityTemplateDo.Category;
                        activity.ComponentActivities = activityTemplateDo.ComponentActivities;
                        activity.Description = activityTemplateDo.Description;
                        activity.Label = activityTemplateDo.Label;
                        activity.MinPaneWidth = activityTemplateDo.MinPaneWidth;
                        activity.Name = activityTemplateDo.Name;
                        activity.NeedsAuthentication = activityTemplateDo.NeedsAuthentication;
                        activity.Tags = activityTemplateDo.Tags;
                        activity.TerminalId = activityTemplateDo.TerminalId;
                        activity.Type = activityTemplateDo.Type;
                        activity.Version = activityTemplateDo.Version;
                        uow.SaveChanges();
                    }
                    
                    _activityTemplates[activityTemplateDo.Id] = Clone(activity);
                }
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
            Initialize();

            lock (_activityTemplates)
            {
                return _activityTemplates.Values.Single(x => x.Name == name && x.Version == version);
            }
        }
    }
}
