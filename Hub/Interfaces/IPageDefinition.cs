using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Entities;

namespace Hub.Interfaces
{
    public interface IPageDefinition
    {
        IEnumerable<PageDefinitionDO> GetAll();

        PageDefinitionDO Get(int id);

        PageDefinitionDO Get(IEnumerable<string> tags);

        IList<PageDefinitionDO> Get(Expression<Func<PageDefinitionDO, bool>> filter);

        void CreateOrUpdate(PageDefinitionDO pageDefinitionDO);

        void Delete(int id);
    }
}