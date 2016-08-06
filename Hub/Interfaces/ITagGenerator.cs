using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Services.PlanDirectory;

namespace Hub.Interfaces
{
    public interface ITagGenerator
    {
        Task<TemplateTagStorage> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId);
    }
}