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

        public static void RegisterActivity<T>(ActivityTemplateDTO activityTemplate)
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

        public static List<ActivityTemplateDTO> GetAllActivities()
        {
            return _activityRegistrations.Select(y => y.Key.ActivityTemplateDTO).ToList();
        }
    }
}
