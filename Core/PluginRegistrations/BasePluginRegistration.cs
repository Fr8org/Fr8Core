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

        public IEnumerable<ActionTemplateDO> AvailableActions
        {
            get
            {
                // return JsonConvert.DeserializeObject<IEnumerable<ActionTemplateDO>>(availableActions,
                //  new JsonSerializerSettings());
                var curActionTemplates = new List<ActionTemplateDO>();
                //IEnumerable<ActionTemplateDO> curActionTemplates;
                //return Mapper.Map(availableActions, curActionTemplates);
                
                foreach (var item in availableActions.ActionNames)
                {
                    var curActionRegistratoin = new ActionTemplateDO();
                    Mapper.Map(item,curActionRegistratoin);
                    curActionTemplates.Add(curActionRegistratoin);
                }
                return curActionTemplates;
            }
        }

        public static IPluginRegistration GetPluginType(ActionDO curAction)
        {
            var pluginRegistrationType = Type.GetType("Core.PluginRegistrations." + curAction.ParentPluginRegistration);
            if (pluginRegistrationType == null)
                throw new ArgumentException(string.Format("Can't find plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            var pluginRegistration = Activator.CreateInstance(pluginRegistrationType) as IPluginRegistration;
            if (pluginRegistration == null)
                throw new ArgumentException(string.Format("Can't find a valid plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            return pluginRegistration;
        }

        public void RegisterActions()
        {
            IEnumerable<ActionTemplateDO> curAvailableCommands = this.AvailableActions;
            foreach (var action in curAvailableCommands)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string curParentPluginRegistration = this.GetType().Name;
                    if (!uow.ActionTemplateRepository.GetQuery().Where(a => a.ActionType == action.ActionType
                        && a.Version == action.Version && a.ParentPluginRegistration == curParentPluginRegistration).Any())
                    {
                        ActionTemplateDO actionTemplateDo = new ActionTemplateDO(action.ActionType,
                                                                        curParentPluginRegistration,
                                                                        action.Version);
                        uow.ActionTemplateRepository.Add(actionTemplateDo);
                        uow.SaveChanges();
                    }
                }
            }
        }


        public string CallPluginRegistrationByString(string typeName, string methodName, Data.Entities.ActionTemplateDO curActionTemplateDo)
        {
            // Get the Type for the class
            Type calledType = Type.GetType(typeName);
            MethodInfo curMethodInfo = calledType.GetMethod(methodName);
            object curObject = Activator.CreateInstance(calledType);
            return (string)curMethodInfo.Invoke(curObject, new Object[] { curActionTemplateDo });
        }

        public string AssembleName(Data.Entities.ActionTemplateDO curActionTemplateDo)
        {
            return string.Format("Core.PluginRegistrations.{0}PluginRegistration_v{1}", curActionTemplateDo.ParentPluginRegistration, curActionTemplateDo.Version);
        }

        public virtual Task<IEnumerable<string>> GetFieldMappingTargets(Data.Entities.ActionDO curAction)
        {
            throw new NotImplementedException();
        }
    }
}