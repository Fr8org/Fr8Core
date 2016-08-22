using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IPlanTemplateDetailsGenerator
    {
        Task Generate(PublishPlanTemplateDTO publishPlanTemplateDto);
        Task<bool> HasGeneratedPage(PublishPlanTemplateDTO planTemplate);
    }
}