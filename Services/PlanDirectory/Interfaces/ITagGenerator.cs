using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using PlanDirectory.Infrastructure;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Interfaces
{
    public interface ITagGenerator
    {
        Task<List<TemplateTag>> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId);
    }
}