using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Interfaces
{
    /// <summary>
    /// Defines an activity that can executed in context of .NET SDK for Terminals
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IActivity.md
    /// </summary>
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
