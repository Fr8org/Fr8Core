using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Data.Manifests;

namespace Hub.Interfaces
{
    public interface IWebservicesPageGenerator
    {
        Task Generate(PlanTemplateCM planTemplate, string fr8AccountId);
        Task Generate(PageDefinitionDO pageDefinition, string fr8AccountId);

        Task<bool> HasGeneratedPage(PageDefinitionDO pageDefinition);
    }
}