using System.Threading.Tasks;
using fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Interfaces
{
    public interface IActivity
    {
        Task Run(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext);
        Task RunChildActivities(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext);
        Task Configure(ActivityContext activityContext);
        Task Activate(ActivityContext activityContext);
        Task Deactivate(ActivityContext activityContext);
        Task<DocumentationResponseDTO> GetDocumentation(ActivityContext activityContext, string documentationType);
    }
}
