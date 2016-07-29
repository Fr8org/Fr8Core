using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

namespace PlanDirectory.Interfaces
{
    public interface IWebservicesPageGenerator
    {
        Task Generate(PlanTemplateCM planTemplate, string fr8AccountId);
    }
}