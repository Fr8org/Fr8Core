using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using HubWeb.Infrastructure_PD.Infrastructure;

namespace HubWeb.Infrastructure_PD.Interfaces
{
    public interface ITagGenerator
    {
        Task<TemplateTagStorage> GetTags(PlanTemplateCM planTemplateCM, string fr8AccountId);
    }
}