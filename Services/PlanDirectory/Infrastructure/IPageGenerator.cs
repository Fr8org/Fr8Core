using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Infrastructure
{
    public interface IPageGenerator
    {
        Task Generate(TemplateTagStorage storage, PlanTemplateCM planTemplate, IList<PageDefinitionDO> pageDefinitions, string fr8AccountId);
    }
}