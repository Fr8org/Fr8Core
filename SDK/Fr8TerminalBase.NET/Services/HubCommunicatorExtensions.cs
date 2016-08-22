using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Services
{
    public static class HubCommunicatorExtensions
    {
        public static async Task<ActivityTemplateDTO> GetActivityTemplate(this IHubCommunicator that, Guid activityTemplateId)
        {
            var allActivityTemplates = await that.GetActivityTemplates();

            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Id == activityTemplateId);


            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. Id: {activityTemplateId}");
            }

            return foundActivity;
        }

        public static async Task<ActivityTemplateDTO> GetActivityTemplate(this IHubCommunicator that, string terminalName, string activityTemplateName, string activityTemplateVersion = "1", string terminalVersion = "1")
        {
            var allActivityTemplates = await that.GetActivityTemplates();

            var foundActivity = allActivityTemplates.FirstOrDefault(a =>
                a.Terminal.Name == terminalName && a.Terminal.Version == terminalVersion &&
                a.Name == activityTemplateName && a.Version == activityTemplateVersion);

            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. TerminalName: {terminalName}\nTerminalVersion: {terminalVersion}\nActivitiyTemplateName: {activityTemplateName}\nActivityTemplateVersion: {activityTemplateVersion}");
            }

            return foundActivity;
        }

        public static async Task<ActivityPayload> ConfigureChildActivity(this IHubCommunicator that, ActivityPayload parent, ActivityPayload child)
        {
            var result = await that.ConfigureActivity(child);

            parent.ChildrenActivities.Remove(child);
            parent.ChildrenActivities.Add(result);

            return result;
        }

        public static async Task<ActivityPayload> AddAndConfigureChildActivity(this IHubCommunicator that, Guid parentActivityId, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)
        {
            //assign missing properties
            label = string.IsNullOrEmpty(label) ? activityTemplate.Label : label;
            name = string.IsNullOrEmpty(name) ? activityTemplate.Label : label;
            return await that.CreateAndConfigureActivity(activityTemplate.Id, name, order, parentActivityId);
        }

        public static async Task<ActivityPayload> AddAndConfigureChildActivity(this IHubCommunicator that, ActivityPayload parentActivity, ActivityTemplateDTO activityTemplate, string name = null, string label = null, int? order = null)
        {
            var child = await AddAndConfigureChildActivity(that, parentActivity.Id, activityTemplate, name, label, order);
            parentActivity.ChildrenActivities.Add(child);
            return child;
        }
    }
}
