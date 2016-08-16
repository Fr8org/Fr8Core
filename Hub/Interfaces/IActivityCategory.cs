using System;
using Data.Entities;

namespace Hub.Interfaces
{
    public interface IActivityCategory
    {
        ActivityCategoryDO RegisterOrUpdate(ActivityCategoryDO activityCategory);
        ActivityCategoryDO GetById(Guid id);
        ActivityCategoryDO GetByName(string name, bool throwIfNotFound = true);
    }
}
