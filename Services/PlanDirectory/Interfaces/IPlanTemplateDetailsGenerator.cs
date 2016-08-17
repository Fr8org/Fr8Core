using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace PlanDirectory.Interfaces
{
    public interface IPlanTemplateDetailsGenerator
    {
        Task Generate(PublishPlanTemplateDTO publishPlanTemplateDto);
    }
}