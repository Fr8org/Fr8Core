using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Services
{
    /// <summary>
    /// Service that stores and manages information about the current terminal and registered activities
    /// Service is registered as a singleton within the DI container.This service is available globally.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IActivityStore.md
    /// </summary>
    public class ActivityStore : IActivityStore
    {
        private readonly ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory> _activityRegistrations = new ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory>();

        public TerminalDTO Terminal { get; }

        public ActivityStore(TerminalDTO terminal)
        {
            Terminal = terminal;
        }

        public void RegisterActivity(ActivityTemplateDTO activityTemplate, IActivityFactory activityFactory)
        {
            if (!_activityRegistrations.TryAdd(new ActivityRegistrationKey(activityTemplate), activityFactory))
            {
                throw new Exception("Unable to add ActivityRegistration to Dictionary");
            }
        }

        /// <summary>
        /// Registers activity with default Activity Factory
        /// </summary>
        /// <typeparam name="T">Type of activity</typeparam>
        /// <param name="activityTemplate"></param>
        public void RegisterActivity<T>(ActivityTemplateDTO activityTemplate) where T : IActivity
        {
            RegisterActivity(activityTemplate, new DefaultActivityFactory(typeof(T)));
        }

        public IActivityFactory GetFactory(string name, string version)
        {
            IActivityFactory factory;
            if (!_activityRegistrations.TryGetValue(new ActivityRegistrationKey(name, version), out factory))
            {
                return null;
            }
            return factory;
        }
        
        public List<ActivityTemplateDTO> GetAllTemplates()
        {
            return _activityRegistrations.Select(x=>x.Key.ActivityTemplateDTO).ToList();
        }
    }
}
