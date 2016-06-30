using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using StructureMap;

namespace Fr8.TerminalBase.Services
{
    public class ActivityStore : IActivityStore
    {
        private readonly ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory> _activityRegistrations = new ConcurrentDictionary<ActivityRegistrationKey, IActivityFactory>();
        private readonly IContainer _container;

        public TerminalDTO Terminal { get; }

        public ActivityStore(TerminalDTO terminal, IContainer container)
        {
            Terminal = terminal;
            _container = container;

            Terminal.PublicIdentifier = CloudConfigurationManager.GetSetting("TerminalId") ?? ConfigurationManager.AppSettings[terminal.Name + "TerminalId"];
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

        public IActivityFactory GetFactory(ActivityTemplateDTO activityTemplate)
        {
            IActivityFactory factory;
            if (!_activityRegistrations.TryGetValue(new ActivityRegistrationKey(activityTemplate), out factory))
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
