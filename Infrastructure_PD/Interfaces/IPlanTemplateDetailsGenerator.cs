using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Infrastructure_PD.Interfaces
{
    public interface IPlanTemplateDetailsGenerator
    {
        Task Generate(PublishPlanTemplateDTO publishPlanTemplateDto);
    }
}