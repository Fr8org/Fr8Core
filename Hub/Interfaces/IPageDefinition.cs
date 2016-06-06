using Data.Entities;

namespace Hub.Interfaces
{
    public interface IPageDefinition
    {
        void CreateOrUpdate(PageDefinitionDO pageDefinitionDO);
    }
}