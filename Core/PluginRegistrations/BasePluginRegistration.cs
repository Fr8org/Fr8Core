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
        private readonly string pluginRegistrationName;

        protected BasePluginRegistration(ActionNameListDTO curAvailableActions,
            string curBaseUrl, string curPluginRegistrationName)
        {
            //AutoMapperBootStrapper.ConfigureAutoMapper();

            availableActions = curAvailableActions;
            baseUrl = curBaseUrl;
            pluginRegistrationName = curPluginRegistrationName;
            //  _action = ObjectFactory.GetInstance<IAction>();
        }

        public string BaseUrl
        {
            get { return baseUrl; }

            set { }
        }

        public IEnumerable<ActivityTemplateDO> AvailableActions
        {
            get
            {
                var curParentPluginRegistration = pluginRegistrationName;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var curActionTemplates = uow.ActivityTemplateRepository
                        .GetQuery()
                        .Where(x => x.DefaultEndPoint == curParentPluginRegistration)
                        .ToList();

                    return curActionTemplates;
                }
            }
        }

        private IEnumerable<ActivityTemplateDO> ActionsToBeRegistered
        {
            get
            {
                var curActionTemplates = new List<ActivityTemplateDO>();
                
                foreach (var item in availableActions.ActionNames)
                {
                    var curActionRegistration = new ActivityTemplateDO();
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

            var pluginRegistrationType = Type.GetType(AssembleName(curAction.ActionTemplate));
            if (pluginRegistrationType == null)
            {
                throw new ArgumentException(string.Format("Can't find plugin registration type: {0}", curAction.ActionTemplate.DefaultEndPoint), "curAction");
            }

            return Activator.CreateInstance(pluginRegistrationType) as IPluginRegistration;
        }

        public void RegisterActions()
        {
            IEnumerable<ActivityTemplateDO> curAvailableCommands = this.ActionsToBeRegistered;
            foreach (var action in curAvailableCommands)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    // string curParentPluginRegistration = this.GetType().Name;
                    if (!uow.ActivityTemplateRepository.GetQuery().Where(a => a.Name == action.Name
                        && a.Version == action.Version && a.DefaultEndPoint == pluginRegistrationName).Any())
                    {
                        ActivityTemplateDO actionTemplateDo = new ActivityTemplateDO(action.Name,
                                                                        pluginRegistrationName,
                                                                        action.Version);
                        uow.ActivityTemplateRepository.Add(actionTemplateDo);
                        uow.SaveChanges();
                    }
                }
            }
        }


        public string CallPluginRegistrationByString(string typeName, string methodName, Data.Entities.ActionDO curActionDO)
        {
            // Get the Type for the class
            Type calledType = Type.GetType(typeName);
            MethodInfo curMethodInfo = calledType.GetMethod(methodName);
            object curObject = Activator.CreateInstance(calledType);
            return (string)curMethodInfo.Invoke(curObject, new Object[] { curActionDO });
        }

        string IPluginRegistration.AssembleName(Data.Entities.ActivityTemplateDO curActionTemplateDO)
        {
            return AssembleName(curActionTemplateDO);
        }

        public static string AssembleName(Data.Entities.ActivityTemplateDO curActionTemplateDo)
        {
            return string.Format("Core.PluginRegistrations.{0}PluginRegistration_v{1}", curActionTemplateDo.DefaultEndPoint, curActionTemplateDo.Version);
        }

        public virtual Task<IEnumerable<string>> GetFieldMappingTargets(Data.Entities.ActionDO curAction)
        {
            throw new NotImplementedException();
        }
    }
}