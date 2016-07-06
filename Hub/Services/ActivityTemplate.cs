using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Utility;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class ActivityTemplate : IActivityTemplate
    {
        private readonly ITerminal _terminal;
        private readonly IActivityCategory _activityCategory;
        private readonly Dictionary<Guid, ActivityTemplateDO> _activityTemplates = new Dictionary<Guid, ActivityTemplateDO>();
        private bool _isInitialized;

        public bool IsATandTCacheDisabled
        {
            get;
            private set;
        }
        
        public ActivityTemplate(ITerminal terminal, IActivityCategory activityCategory)
        {
            IsATandTCacheDisabled = string.Equals(CloudConfigurationManager.GetSetting("DisableATandTCache"), "true", StringComparison.InvariantCultureIgnoreCase);
            _terminal = terminal;
            _activityCategory = activityCategory;
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
                var query = uow.ActivityTemplateRepository.GetQuery()
                    .Include(x => x.WebService)
                    .Include(x => x.Categories)
                    .Include("Categories.ActivityCategory");
                foreach (var activityTemplate in query)
                {
                    _activityTemplates[activityTemplate.Id] = Clone(activityTemplate);
                }
            }
        }

        public ActivityTemplateInfo GetActivityTemplateInfo(string fullActivityTemplateName)
        {
            if (string.IsNullOrEmpty(fullActivityTemplateName))
            {
                throw new ApplicationException("Full ActivityTemplate name is not specified.");
            }

            string name;
            string version = null;

            var tokens = fullActivityTemplateName.Split('_');
            var lastToken = tokens[tokens.Length - 1];
            if (lastToken.StartsWith("v"))
            {
                int versionValue;
                if (Int32.TryParse(lastToken.Substring(1), out versionValue))
                {
                    version = versionValue.ToString();
                }
            }

            if (version != null)
            {
                name = fullActivityTemplateName.Substring(0, fullActivityTemplateName.Length - lastToken.Length - 1);
            }
            else
            {
                name = fullActivityTemplateName;
            }

            return new ActivityTemplateInfo()
            {
                Name = name,
                Version = version
            };
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

            if (source.Categories != null)
            {
                newTemplate.Categories = new List<ActivityCategorySetDO>();
                foreach (var acs in source.Categories)
                {
                    var newActivityCategory = new ActivityCategoryDO();
                    var activityCategory = _activityCategory.GetById(acs.ActivityCategoryId);
                    CopyPropertiesHelper.CopyProperties(activityCategory, newActivityCategory, false);

                    newTemplate.Categories.Add(new ActivityCategorySetDO()
                    {
                        ActivityTemplateId = newTemplate.Id,
                        ActivityTemplate = newTemplate,
                        ActivityCategoryId = newActivityCategory.Id,
                        ActivityCategory = newActivityCategory
                    });
                }
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

        private List<ActivityCategorySetDO> ApplyActivityCategories(
            IUnitOfWork uow, 
            ActivityTemplateDO activityTemplate,
            List<ActivityCategoryDO> activityCategories)
        {
            // Remove previously registered ActivityCategorySets.
            var existingActivityCategorySets = uow.ActivityCategorySetRepository
                .GetQuery()
                .Where(x => x.ActivityTemplateId == activityTemplate.Id)
                .ToList();

            foreach (var activityCategorySet in existingActivityCategorySets)
            {
                uow.ActivityCategorySetRepository.Remove(activityCategorySet);
            }

            // Add new activityCategorySets.
            var activityCategorySets = new List<ActivityCategorySetDO>();

            foreach (var activityCategory in activityCategories)
            {
                var registeredActivityCategory = _activityCategory.RegisterOrUpdate(activityCategory);

                var activityCategorySet = new ActivityCategorySetDO()
                {
                    Id = Guid.NewGuid(),
                    ActivityTemplateId = activityTemplate.Id,
                    ActivityCategoryId = registeredActivityCategory.Id
                };

                activityCategorySets.Add(activityCategorySet);
                uow.ActivityCategorySetRepository.Add(activityCategorySet);
            }

            uow.SaveChanges();

            return activityCategorySets;
        }

        public void RegisterOrUpdate(ActivityTemplateDO activityTemplateDo)
        {
            if (activityTemplateDo == null)
            {
                return;
            }

            // validate values
            if (string.IsNullOrWhiteSpace(activityTemplateDo.Name))
            {
                throw new ArgumentOutOfRangeException("ActivityTemplate.Name can't be empty");
            }

            if (string.IsNullOrWhiteSpace(activityTemplateDo.Label))
            {
                throw new ArgumentOutOfRangeException("ActivityTemplate.Label can't be empty");
            }

            if (string.IsNullOrWhiteSpace(activityTemplateDo.Version))
            {
                throw new ArgumentOutOfRangeException("ActivityTemplate.Version can't be empty");
            }

            int tempVersion;
            if (!int.TryParse(activityTemplateDo.Version, out tempVersion))
            {
                throw new ArgumentOutOfRangeException($"ActivityTemplate.Version is not a valid integer value: {activityTemplateDo.Version}");
            }

            if (activityTemplateDo.WebService == null)
            {
                throw new ArgumentOutOfRangeException("ActivityTemplate.WebService is not set");
            }

            if (string.IsNullOrWhiteSpace(activityTemplateDo.WebService.Name))
            {
                throw new ArgumentOutOfRangeException("ActivityTemplate.WebService.Name can't be empty");
            }

            // we are going to change activityTemplateDo. It is not good to corrupt method's input parameters.
            // make a copy
            var clone = new ActivityTemplateDO();
            
            CopyPropertiesHelper.CopyProperties(activityTemplateDo, clone, true);
            
            clone.Terminal = activityTemplateDo.Terminal;

            // Make copy of web-service reference and add it to 
            if (activityTemplateDo.WebService != null)
            {
                var wsClone = new WebServiceDO();
                CopyPropertiesHelper.CopyProperties(activityTemplateDo.WebService, wsClone, true);
                clone.WebService = wsClone;
            }

            // Create list of activity categories for current ActivityTemplate.
            var activityCategories = new List<ActivityCategoryDO>();
            if (activityTemplateDo.Categories != null)
            {
                foreach (var acs in activityTemplateDo.Categories)
                {
                    activityCategories.Add(acs.ActivityCategory);
                }
            }

            activityTemplateDo = clone;
            activityTemplateDo.Terminal = null; // otherwise we can add dupliacte terminals into the DB
            
            if (!IsATandTCacheDisabled)
            {
                Initialize();
            }

            lock (_activityTemplates)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    // Create new WebService entity or update reference to existing WebService entity.
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
                    
                    // Try to extract existing ActivityTemplate.
                    var activity = uow.ActivityTemplateRepository.GetQuery()
                        .Include(x => x.WebService)
                        .FirstOrDefault(t => t.Name == activityTemplateDo.Name
                            && t.TerminalId == activityTemplateDo.TerminalId
                            && t.Version == activityTemplateDo.Version);

                    // We're creating new ActivityTemplate.
                    if (activity == null)
                    {
                        activity = activityTemplateDo;
                        activityTemplateDo.Id = Guid.NewGuid();
                        activityTemplateDo.Categories = null;

                        uow.ActivityTemplateRepository.Add(activityTemplateDo);
                        uow.SaveChanges();

                        activityTemplateDo.Categories = ApplyActivityCategories(uow, activityTemplateDo, activityCategories);
                    }
                    // We're updating existing ActivityTemplate.
                    else
                    {
                        // This is for updating activity template
                        CopyPropertiesHelper.CopyProperties(activityTemplateDo, activity, false, x => x.Name != "Id");
                        activity.ActivityTemplateState = ActivityTemplateState.Active;
                        activity.WebService = activityTemplateDo.WebService;
                        uow.SaveChanges();

                        activity.Categories = ApplyActivityCategories(uow, activity, activityCategories);
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
