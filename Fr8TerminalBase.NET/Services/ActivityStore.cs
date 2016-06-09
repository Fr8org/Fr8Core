using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using StructureMap;

namespace Fr8.TerminalBase.Services
{
    public interface IActivityStore
    {
        void RegisterActivity(ActivityTemplateDTO activityTemplate, IActivityFactory activityFactory);

        /// <summary>
        /// Registers activity with default Activity Factory
        /// </summary>
        /// <typeparam name="T">Type of activity</typeparam>
        /// <param name="activityTemplate"></param>
        void RegisterActivity<T>(ActivityTemplateDTO activityTemplate) where T : IActivity;

        IActivityFactory GetValue(ActivityTemplateDTO activityTemplate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminal">
        /// Terminals on integrations tests share same environment
        /// so we are providing terminal information to seperate terminal activities
        /// </param>
        /// <returns></returns>
        List<ActivityTemplateDTO> GetAllActivities(TerminalDTO terminal);
    }

    public class ActivityStore : IActivityStore
    {
        public readonly ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory> _activityRegistrations = new ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory>();
        private readonly IContainer _container;

        public ActivityStore(IContainer container)
        {
            _container = container;
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
            RegisterActivity(activityTemplate, new DefaultActivityFactory(typeof(T), _container));
        }

        public IActivityFactory GetValue(ActivityTemplateDTO activityTemplate)
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
        public List<ActivityTemplateDTO> GetAllActivities(TerminalDTO terminal)
        {
            return _activityRegistrations
                .Select(y => y.Key.ActivityTemplateDTO)
                .Where(t => t.Terminal.Name == terminal.Name)
                .ToList();
        }
    }
}
