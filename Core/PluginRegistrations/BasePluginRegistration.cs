using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using Utilities;
using System.Reflection;
using Data.Interfaces;
using System.Linq;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        private readonly string availableActions = "";
        private readonly string baseUrl = "";

        public BasePluginRegistration(string curAvailableActions, string curBaseURL)
        {
            availableActions = curAvailableActions;
            baseUrl = curBaseURL;
           // _action = ObjectFactory.GetInstance<IAction>();
        }

        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }

            set { }
        }

        public IEnumerable<ActionRegistrationDO> AvailableCommands
        {
            get
            {
                var result = JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(availableActions, new JsonSerializerSettings());
                return result;
            }
        }

        public virtual void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> curAvailableCommands = this.AvailableCommands;
            foreach (var action in curAvailableCommands)
            {
                //_action.Register(action.ActionType, this.GetType().Name, action.Version);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    if (!uow.ActionRegistrationRepository.GetQuery().Where(a => a.ActionType == action.ActionType
                        && a.Version == action.Version && a.ParentPluginRegistration == this.GetType().Name).Any())
                    {
                        ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO(action.ActionType,
                                                                        this.GetType().Name,
                                                                        action.Version);
                        uow.SaveChanges();
                    }
                }
            }
        }
        public string CallPluginRegistrationByString(string typeName, string methodName, ActionRegistrationDO curActionRegistrationDO)
        {
            // Get the Type for the class
            Type calledType = Type.GetType(typeName);
            MethodInfo curMethodInfo = calledType.GetMethod(methodName);
            object curObject = Activator.CreateInstance(calledType);
            return (string)curMethodInfo.Invoke(curObject, new Object[] { curActionRegistrationDO });
        }

        public string AssembleName(ActionRegistrationDO curActionRegistrationDO)
        {
            return string.Format("Core.PluginRegistrations.{0}PluginRegistration_v{1}", curActionRegistrationDO.ParentPluginRegistration, curActionRegistrationDO.Version);
        }
    }
}