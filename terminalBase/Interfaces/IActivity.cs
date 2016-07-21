using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using TerminalBase.Models;

namespace TerminalBase.Interfaces
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
