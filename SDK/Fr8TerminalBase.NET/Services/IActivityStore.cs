using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that stores and manages information about the current terminal and registered activities
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IActivityStore.md
    /// </summary>
    public interface IActivityStore
    {
        TerminalDTO Terminal { get; }

        void RegisterActivity(ActivityTemplateDTO activityTemplate, IActivityFactory activityFactory);

        /// <summary>
        /// Registers activity with default Activity Factory
        /// </summary>
        /// <typeparam name="T">Type of activity</typeparam>
        /// <param name="activityTemplate"></param>
        void RegisterActivity<T>(ActivityTemplateDTO activityTemplate) where T : IActivity;

        IActivityFactory GetFactory(string name, string version);
        
        List<ActivityTemplateDTO> GetAllTemplates();
    }
}