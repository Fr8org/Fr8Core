using System.Collections.Generic;

namespace PlanDirectory.Infrastructure
{
    public interface IPageGenerator
    {
        void Generate(IEnumerable<WebServiceTemplateTag> tags);
    }
}