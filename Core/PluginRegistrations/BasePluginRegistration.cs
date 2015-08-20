using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using System;
using System.Reflection;
using Data.Interfaces;
using System.Linq;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        private readonly ActionNameListDTO availableActions;
        private readonly string baseUrl;
        // private readonly IAction _action;

        protected BasePluginRegistration(ActionNameListDTO curAvailableActions, string curBaseUrl)
        {
            //AutoMapperBootStrapper.ConfigureAutoMapper();

            availableActions = curAvailableActions;
            baseUrl = curBaseUrl;
            //  _action = ObjectFactory.GetInstance<IAction>();
        }

        public string BaseUrl
        {
            get { return baseUrl; }

            set { }
        }

        public IEnumerable<ActionRegistrationDO> AvailableActions
        {
            get
            {
               // return JsonConvert.DeserializeObject<IEnumerable<ActionRegistrationDO>>(availableActions,
                  //  new JsonSerializerSettings());
                IEnumerable<ActionRegistrationDO> curActionRegistration = null;
               return Mapper.Map(availableActions, curActionRegistration);
            }
        }

        public void RegisterActions()
        {
            IEnumerable<ActionRegistrationDO> curAvailableCommands = this.AvailableActions;
            foreach (var action in curAvailableCommands)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string curParentPluginRegistration = this.GetType().Name;
                    if (!uow.ActionRegistrationRepository.GetQuery().Where(a => a.ActionType == action.ActionType
                        && a.Version == action.Version && a.ParentPluginRegistration == curParentPluginRegistration).Any())
                    {
                        ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO(action.ActionType,
                                                                        curParentPluginRegistration,
                                                                        action.Version);
                        uow.ActionRegistrationRepository.Add(actionRegistrationDO);
                        uow.SaveChanges();
                    }
                }
            }
        }


        public string CallPluginRegistrationByString(string typeName, string methodName, Data.Entities.ActionRegistrationDO curActionRegistrationDO)
        {
            // Get the Type for the class
            Type calledType = Type.GetType(typeName);
            MethodInfo curMethodInfo = calledType.GetMethod(methodName);
            object curObject = Activator.CreateInstance(calledType);
            return (string)curMethodInfo.Invoke(curObject, new Object[] { curActionRegistrationDO });
        }



        public string AssembleName(Data.Entities.ActionRegistrationDO curActionRegistrationDO)
        {
            return string.Format("Core.PluginRegistrations.{0}PluginRegistration_v{1}", curActionRegistrationDO.ParentPluginRegistration, curActionRegistrationDO.Version);
        }

        public virtual Task<IEnumerable<string>> GetFieldMappingTargets(Data.Entities.ActionDO curAction)
        {
            throw new NotImplementedException();
        }
    }
}