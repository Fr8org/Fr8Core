using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fr8Data.DataTransferObjects;
using TerminalBase.Interfaces;
using TerminalBase.Models;

namespace TerminalBase.Services
{

    public static class ActivityStore
    {
        public static readonly ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory> _activityRegistrations = new ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory>();
        public static void RegisterActivity(ActivityTemplateDTO activityTemplate, IActivityFactory activityFactory)
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
        public static void RegisterActivity<T>(ActivityTemplateDTO activityTemplate) where T : IActivity
        {
            RegisterActivity(activityTemplate, new DefaultActivityFactory(typeof(T)));
        }

        public static IActivityFactory GetValue(ActivityTemplateDTO activityTemplate)
        {
            IActivityFactory factory;
            if (!_activityRegistrations.TryGetValue(new ActivityRegistrationKey(activityTemplate), out factory))
            {
                return null;
            }
            return factory;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminal">
        /// Terminals on integrations tests share same environment
        /// so we are providing terminal information to seperate terminal activities
        /// </param>
        /// <returns></returns>
        public static List<ActivityTemplateDTO> GetAllActivities(TerminalDTO terminal)
        {
            return _activityRegistrations
                .Select(y => y.Key.ActivityTemplateDTO)
                .Where(t => t.Terminal.Name == terminal.Name)
                .ToList();
        }
    }
}
