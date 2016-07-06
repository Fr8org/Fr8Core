using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;

namespace Hub.Services
{
    public class ActivityCategory : IActivityCategory
    {
        private readonly Dictionary<Guid, ActivityCategoryDO> _activityCategories =
            new Dictionary<Guid, ActivityCategoryDO>();

        private bool _isInitialized = false;

        public bool IsATandTCacheDisabled { get; private set; }

        public ActivityCategory()
        {
            IsATandTCacheDisabled = string.Equals(
                CloudConfigurationManager.GetSetting("DisableATandTCache"),
                "true",
                StringComparison.InvariantCultureIgnoreCase
            );
        }

        private void Initialize()
        {
            if (_isInitialized && !IsATandTCacheDisabled)
            {
                return;
            }

            lock (_activityCategories)
            {
                if (_isInitialized && !IsATandTCacheDisabled)
                {
                    return;
                }

                if (IsATandTCacheDisabled)
                {
                    _activityCategories.Clear();
                }

                LoadFromDb();

                _isInitialized = true;
            }
        }

        private void LoadFromDb()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var query = uow.ActivityCategoryRepository.GetQuery();
                foreach (var activityCategory in query)
                {
                    _activityCategories[activityCategory.Id] = Clone(activityCategory);
                }
            }
        }

        private ActivityCategoryDO Clone(ActivityCategoryDO activityCategory)
        {
            var newActivityCategory = new ActivityCategoryDO();
            CopyPropertiesHelper.CopyProperties(activityCategory, newActivityCategory, false);

            return newActivityCategory;
        }

        public ActivityCategoryDO RegisterOrUpdate(ActivityCategoryDO activityCategory)
        {
            if (!IsATandTCacheDisabled)
            {
                Initialize();
            }

            lock (_activityCategories)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var categoryNameUpper = activityCategory.Name.ToUpper();
                    var category = uow.ActivityCategoryRepository
                        .GetQuery()
                        .Where(x => x.Name.ToUpper() == categoryNameUpper)
                        .FirstOrDefault();

                    if (category == null)
                    {
                        var newActivityCategory = new ActivityCategoryDO()
                        {
                            Id = Guid.NewGuid(),
                            Name = activityCategory.Name,
                            IconPath = activityCategory.IconPath
                        };

                        uow.ActivityCategoryRepository.Add(newActivityCategory);
                        uow.SaveChanges();

                        category = newActivityCategory;
                    }
                    else
                    {
                        category.Name = activityCategory.Name;
                        category.IconPath = activityCategory.IconPath;
                        uow.SaveChanges();
                    }

                    _activityCategories[category.Id] = Clone(category);

                    return category;
                }
            }
        }

        public ActivityCategoryDO GetById(Guid id)
        {
            Initialize();

            lock (_activityCategories)
            {
                ActivityCategoryDO category;

                if (!_activityCategories.TryGetValue(id, out category))
                {
                    throw new KeyNotFoundException(string.Format("Can't find activity category with id {0}", id));
                }

                return category;
            }
        }

        public ActivityCategoryDO GetByName(string name)
        {
            Initialize();

            lock (_activityCategories)
            {
                var category = _activityCategories.Values.Where(x => x.Name == name).FirstOrDefault();
                if (category == null)
                {
                    throw new KeyNotFoundException(string.Format("Can't find activity category with name {0}", name));
                }

                return category;
            }
        }
    }
}
