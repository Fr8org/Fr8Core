using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Interfaces
{
    public interface IWebservicesPageGenerator
    {
        Task Generate(PlanTemplateCM planTemplate, string fr8AccountId);
    }
}