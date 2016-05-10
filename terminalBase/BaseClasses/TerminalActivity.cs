using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;

namespace TerminalBase.BaseClasses
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

    public class ActivityPayload
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public List<ActivityPayload> ChildrenActivities { get; set; }
        public ActivityTemplateDTO ActivityTemplate { get; set; }
        public ICrateStorage CrateStorage { get; set; }
        public Guid? RootPlanNodeId { get; set; }
        public Guid? ParentPlanNodeId { get; set; }
        public int Ordering { get; set; }
    }

    public class DefaultActivityFactory : IActivityFactory
    {
        private readonly Type _type;

        public DefaultActivityFactory(Type type)
        {
            _type = type;
        }

        public IActivity Create()
        {
            return (IActivity)Activator.CreateInstance(_type);
        }

        public IActivity Create(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext)
        {
            return (IActivity)Activator.CreateInstance(_type, activityContext, containerExecutionContext);
        }

        public IActivity Create(ActivityContext activityContext)
        {
            return (IActivity)Activator.CreateInstance(_type, activityContext, null);
        }
    }

    // This is for data requiered for generic activity requests processing
    // We use dedicated class to avoid ugly Win32 API-like methods with enormous number of parameters
    // Also this will help to add new parameters without forcing ALL activities to be rewritten beacuse of signature change.
    public class ActivityContext
    {
        public ActivityPayload ActivityPayload { get; set; }
        public AuthorizationToken AuthorizationToken { get; }
        public string UserId { get; }
    }

    //just a wrapper for potential changes in future
    public class TerminalActivityResponse
    {
        public ActivityResponseDTO Response { get; set; }
    }

    public class ContainerExecutionContext
    {
        public ICrateStorage PayloadStorage { get; set; }
        public Guid ContainerId { get; set; }
    }

    public class TerminalPayload
    {

    }

    public interface IActivity
    {
        Task Run();
        Task Configure();
        Task Activate();
        Task Deactivate();
    }

    public interface IActivityFactory
    {
        // During processing requests activity can modify data in activityContext and containerExecutionContext.
        // As we pass all data by reference, terminal controller will be able to correctly serialize changed data into responses for Hub.
        IActivity Create(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext);
        IActivity Create(ActivityContext activityContext);
    }

    public class AuthorizationToken
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalDomainId { get; set; }
        public string UserId { get; set; }
        public string ExternalStateToken { get; set; }
        public string AdditionalAttributes { get; set; }
        public string Error { get; set; }
        public bool AuthCompletedNotificationRequired { get; set; }
        public int TerminalID { get; set; }
    }

    public struct ActivityRegistrationKey
    {
        public readonly ActivityTemplateDTO ActivityTemplateDTO;

        public ActivityRegistrationKey(ActivityTemplateDTO activityTemplateDTO)
        {
            ActivityTemplateDTO = activityTemplateDTO;
        }

        public bool Equals(ActivityRegistrationKey other)
        {
            return string.Equals(ActivityTemplateDTO.Name, other.ActivityTemplateDTO.Name) && string.Equals(ActivityTemplateDTO.Version, other.ActivityTemplateDTO.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ActivityRegistrationKey && Equals((ActivityRegistrationKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ActivityTemplateDTO.Name?.GetHashCode() ?? 0) * 397) ^ (ActivityTemplateDTO.Version?.GetHashCode() ?? 0);
            }
        }
    }



}
