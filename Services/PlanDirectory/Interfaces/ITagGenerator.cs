using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8Data.Manifests;
using System.Threading.Tasks;
using PlanDirectory.Infrastructure;

namespace PlanDirectory.Interfaces
{
    public interface ITagGenerator
    {
        Task<List<TemplateTag>> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId);
    }
}