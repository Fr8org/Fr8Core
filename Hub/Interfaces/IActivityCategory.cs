using Data.Entities;

namespace Hub.Interfaces
{
    public interface IActivityCategory
    {
        ActivityCategoryDO RegisterOrUpdate(ActivityCategoryDO activityCategory);
    }
}
