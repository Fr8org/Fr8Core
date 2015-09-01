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
                var curParentPluginRegistration = this.GetType().Name;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var curActionTemplates = uow.ActionTemplateRepository
                        .GetQuery()
                        .Where(x => x.ParentPluginRegistration == curParentPluginRegistration)
                        .ToList();

                    return curActionTemplates;
                }
            }
        }

        private IEnumerable<ActionTemplateDO> ActionsToBeRegistered
        {
            get
            {
                var curActionTemplates = new List<ActionTemplateDO>();
                
                foreach (var item in availableActions.ActionNames)
                {
                    var curActionRegistration = new ActionTemplateDO();
                    Mapper.Map(item, curActionRegistration);
                    curActionTemplates.Add(curActionRegistration);
                }

                return curActionTemplates;
            }
        }

        public static IPluginRegistration GetPluginType(ActionDO curAction)
        {
            if (curAction.ActionTemplate == null)
            {
                throw new ArgumentException("ActionTemplate is not specified for current action.");
            }

            var pluginRegistrationType = Type.GetType("Core.PluginRegistrations." + curAction.ActionTemplate.ParentPluginRegistration);
            if (pluginRegistrationType == null)
            {
                throw new ArgumentException(string.Format("Can't find plugin registration type: {0}", curAction.ActionTemplate.ParentPluginRegistration), "curAction");
            }

            var pluginRegistration = Activator.CreateInstance(pluginRegistrationType) as IPluginRegistration;
            if (pluginRegistration == null)
            {
                throw new ArgumentException(string.Format("Can't find a valid plugin registration type: {0}", curAction.ParentPluginRegistration), "curAction");
            }

            return pluginRegistration;
        }

        public void RegisterActions()
        {
            IEnumerable<ActionTemplateDO> curAvailableCommands = this.ActionsToBeRegistered;
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