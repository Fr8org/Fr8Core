using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Infrastructure
{
    public interface IPageGenerator
    {
        Task Generate(IEnumerable<TemplateTag> tags, PlanTemplateCM planTemplate);
    }
}