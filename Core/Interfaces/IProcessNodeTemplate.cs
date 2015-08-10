using System.Collections.Generic;
using Data.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// ProcessNodeTemplate service.
    /// </summary>
    public interface IProcessNodeTemplate
    {
        void Create(ProcessNodeTemplateDO processNodeTemplate);
        void Update(ProcessNodeTemplateDO processNodeTemplate);
        ProcessNodeTemplateDO Remove(int id);
    }
}
