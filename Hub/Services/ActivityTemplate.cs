using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Utility;
using Hub.Interfaces;
using StructureMap;
using Utilities.Configuration.Azure;

namespace Hub.Services
{
    public class ActivityTemplate : IActivityTemplate
    {
        public const string EmailDelivererTag = "Email Deliverer";

        public const string TableDataGeneratorTag = "Table Data Generator";

        private readonly ITerminal _terminal;
        private readonly Dictionary<Guid, ActivityTemplateDO> _activityTemplates = new Dictionary<Guid, ActivityTemplateDO>();
        private bool _isInitialized;

        public bool IsATandTCacheDisabled
        {
            get;
            private set;
        }
        
        public ActivityTemplate(ITerminal terminal)
        {
            IsATandTCacheDisabled = string.Equals(CloudConfigurationManager.GetSetting("DisableATandTCache"), "true", StringComparison.InvariantCultureIgnoreCase);
            _terminal = terminal;
        }

        private void Initialize()
        {
            if (_isInitialized && !IsATandTCacheDisabled)
            {
                return;
            }

            lock (_activityTemplates)
            {
                if (_isInitialized && !IsATandTCacheDisabled)
                {
                    return;
                }

                if (IsATandTCacheDisabled)
                {
                    _activityTemplates.Clear();
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

        public string GetTerminalUrl(Guid? curActivityTemplateId)
        {
            if (curActivityTemplateId == null)
            {
                return null;
            }

            Initialize();

            return GetByKey(curActivityTemplateId.Value).Terminal.Endpoint;
        }

        public ActivityTemplateDO GetByKey(Guid curActivityTemplateId)
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

            CopyPropertiesHelper.CopyProperties(source, newTemplate, false);

            newTemplate.Terminal = _terminal.GetByKey(source.TerminalId);
          
            if (source.WebService != null)
            {
                var webService = new WebServiceDO();
                
                CopyPropertiesHelper.CopyProperties(source.WebService, webService, false);
                
                newTemplate.WebService = webService;
            }

            return newTemplate;
        }

        public void RemoveInactiveActivities(List<ActivityTemplateDO> activityTemplates)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //let's first get webservice of those activities
                var webService = activityTemplates.FirstOrDefault(a => a.WebService != null)?.WebService;
                if (webService == null)
                {
                    //try to load webservice from db
                    var dummyActivity = activityTemplates.First();
                    webService = uow.ActivityTemplateRepository.GetQuery()
                            .FirstOrDefault(t => t.Name == dummyActivity.Name)?
                            .WebService;
                }
                else
                {
                    webService = uow.WebServiceRepository.GetQuery().FirstOrDefault(t => t.Name == webService.Name);
                }

                //we can't operate without a common webservice for those activities
                if (webService == null)
                {
                    //wow we can't do anything about this
                    return;
                }

                var currentActivityNames = activityTemplates.Select(x => x.Name);
                //get activities which we didn't receive as parameter
                var inactiveActivities = uow.ActivityTemplateRepository.GetQuery().Where(t => !currentActivityNames.Contains(t.Name) && t.WebServiceId == webService.Id).ToList();

                //we need to remove those inactiveActivities both from db and cache
                foreach (var activityTemplateDO in inactiveActivities)
                {
                    activityTemplateDO.ActivityTemplateState = ActivityTemplateState.Inactive;
                    _activityTemplates.Remove(activityTemplateDO.Id);
                }
                uow.SaveChanges();
            }
          
        }

        public void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo)
        {
            if (activityTemplateDo == null)
            {
                return;
            }

            // we are going to change activityTemplateDo. It is not good to corrupt method's input parameters.
            // make a copy
            var clone = new ActivityTemplateDO();
            
            CopyPropertiesHelper.CopyProperties(activityTemplateDo, clone, true);
            
            clone.Terminal = activityTemplateDo.Terminal;

            if (activityTemplateDo.WebService != null)
            {
                var wsClone = new WebServiceDO();
                CopyPropertiesHelper.CopyProperties(activityTemplateDo.WebService, wsClone, true);
                clone.WebService = wsClone;
            }

            activityTemplateDo = clone;

            var registeredTerminal = _terminal.RegisterOrUpdate(activityTemplateDo.Terminal);
            
            activityTemplateDo.Terminal = null; // otherwise we can add dupliacte terminals into the DB

            if (registeredTerminal != null)
            {
                activityTemplateDo.TerminalId = registeredTerminal.Id;
            }

            if (!IsATandTCacheDisabled)
            {
                Initialize();
            }

            lock (_activityTemplates)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    if (activityTemplateDo.WebService != null)
                    {
                        var existingWebService = uow.WebServiceRepository.FindOne(x => x.Name == activityTemplateDo.WebService.Name);

                        if (existingWebService != null)
                        {
                            activityTemplateDo.WebServiceId = existingWebService.Id;
                            activityTemplateDo.WebService = existingWebService;
                        }
                        else
                        {
                            activityTemplateDo.WebService.Id = 0;
                            activityTemplateDo.WebServiceId = 0;
                        }
                    }
                    
                    var activity = uow.ActivityTemplateRepository.GetQuery().Include(x => x.WebService).FirstOrDefault(t => t.Name == activityTemplateDo.Name
                                                                                                                         && t.TerminalId == activityTemplateDo.TerminalId
                                                                                                                         && t.Version == activityTemplateDo.Version);

                    if (activity == null)
                    {
                        activityTemplateDo.Id = Guid.NewGuid();
                        uow.ActivityTemplateRepository.Add(activity = activityTemplateDo);
                        uow.SaveChanges();
                    }
                    else
                    {
                        // This is for updating activity template
                        CopyPropertiesHelper.CopyProperties(activityTemplateDo, activity, false, x => x.Name != "Id");
                        activity.ActivityTemplateState = ActivityTemplateState.Active;
                        activity.WebService = activityTemplateDo.WebService;
                        uow.SaveChanges();
                    }

                    _activityTemplates[activity.Id] = Clone(activity);
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

            return GetByNameAndVersion(namePart, versionPart);
        }

        /// <summary>
        /// Returns ActivityTemplate by it's name and version.
        /// For example GetByNameAndVersion(uow, "AddPayloadManually", "1").
        /// </summary>
        public ActivityTemplateDO GetByNameAndVersion(string name, string version)
        {
            Initialize();

            lock (_activityTemplates)
            {
                return _activityTemplates.Values.FirstOrDefault(x => x.Name == name && x.Version == version);
            }
        }
    }
}
