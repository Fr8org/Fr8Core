using System.Collections.Generic;
using Data.Entities;

namespace Hub.Interfaces
{
    public interface IPageDefinition
    {
        IEnumerable<PageDefinitionDO> GetAll();

        PageDefinitionDO Get(int id);

        void CreateOrUpdate(PageDefinitionDO pageDefinitionDO);

        void Delete(int id);
    }
}