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
        TerminalDTO Terminal { get; }

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
        List<ActivityTemplateDTO> GetAllActivities();
    }

    public class ActivityStore : IActivityStore
    {
        public readonly ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory> _activityRegistrations = new ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory>();
        private readonly IContainer _container;

        public TerminalDTO Terminal { get; }

        public ActivityStore(TerminalDTO terminal, IContainer container)
        {
            Terminal = terminal;
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
        
        public List<ActivityTemplateDTO> GetAllActivities()
        {
            return _activityRegistrations.Select(x=>x.Key.ActivityTemplateDTO).ToList();
        }
    }
}
