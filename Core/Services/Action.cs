using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using StructureMap;
using Utilities.Serializers.Json;

namespace Core.Services
{
    public class Action : IAction
    {
        private readonly ISubscription _subscription;
        private IPluginRegistration _pluginRegistration;
        private IEnvelope _envelope;
        private IDocuSignTemplate _docusignTemplate; //TODO: switch to wrappers
        private Task curAction;
        private IPluginRegistration _basePluginRegistration;
        private IPlugin _plugin;

        public Action()
        {
            _subscription = ObjectFactory.GetInstance<ISubscription>();
            _pluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
            _plugin = ObjectFactory.GetInstance<IPlugin>();
            _envelope = ObjectFactory.GetInstance<IEnvelope>();
           
            _basePluginRegistration = ObjectFactory.GetInstance<IPluginRegistration>();
        }

        public IEnumerable<TViewModel> GetAllActions<TViewModel>()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetAll().Select(Mapper.Map<TViewModel>);
            }
        }

        public IEnumerable<ActionTemplateDO> GetAvailableActions(IDockyardAccountDO curAccount)
        {
            List<ActionTemplateDO> curActionTemplates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curActionTemplates = uow.ActionTemplateRepository.GetAll().ToList();
            }

            //we're currently bypassing the subscription logic until we need it
            //we're bypassing the pluginregistration logic here because it's going away in V2

            //var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            //var plugins = _plugin.GetAll();
           // var curActionTemplates = plugins
            //    .SelectMany(p => p.AvailableActions)
            //    .OrderBy(s => s.ActionType);

            return curActionTemplates;
        }

        public bool SaveOrUpdateAction(ActionDO currentActionDo)
        {
            ActionDO existingActionDo = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (currentActionDo.ActionTemplateId == 0)
                    currentActionDo.ActionTemplateId = null;

                if (currentActionDo.Id > 0)
                {
                    existingActionDo = uow.ActionRepository.GetByKey(currentActionDo.Id);
                }

                if (currentActionDo.IsTempId)
                {
                    currentActionDo.Id = 0;
                }

                if (existingActionDo != null)
                {
                    existingActionDo.ParentActionList = currentActionDo.ParentActionList;
                    existingActionDo.ParentActionListId = currentActionDo.ParentActionListId;
                    existingActionDo.ActionTemplateId = currentActionDo.ActionTemplateId;
                    existingActionDo.Name = currentActionDo.Name;
                    existingActionDo.ConfigurationSettings = currentActionDo.ConfigurationSettings;
                    existingActionDo.FieldMappingSettings = currentActionDo.FieldMappingSettings;
                    existingActionDo.ParentPluginRegistration = currentActionDo.ParentPluginRegistration;
                }
                else
                {
                    uow.ActionRepository.Add(currentActionDo);
                }
                uow.SaveChanges();

                return true;
            }
        }

        public ActionDO GetById(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetByKey(id);
            }
        }

        public string GetConfigurationSettings(
            ActionTemplateDO curActionTemplateDo)
        {
            if (curActionTemplateDo != null)
            {
                var pluginRegistrationName = _pluginRegistration.AssembleName(curActionTemplateDo);
                var curConfigurationSettingsJson =
                    _pluginRegistration.CallPluginRegistrationByString(pluginRegistrationName,
                        "GetConfigurationSettings", curActionTemplateDo);

                return curConfigurationSettingsJson;
            }
            else
            {
                throw new ArgumentNullException("ActionTemplateDO");
            }
        }

        public void Delete(int id)
        {
			var entity = new ActionDO { Id = id };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Attach(entity);
                uow.ActionRepository.Remove(entity);
                uow.SaveChanges();
            }
        }

        public async Task<int> Process(ActionDO curAction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //if status is unstarted, change it to in-process. If status is completed or error, throw an exception.
                if (curAction.ActionState == ActionState.Unstarted || curAction.ActionState == ActionState.InProcess)
                {
                    curAction.ActionState = ActionState.InProcess;
                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();

                    EventManager.ActionStarted(curAction);
                    var pluginRegistration = BasePluginRegistration.GetPluginType(curAction);
                    var baseUri = new Uri(pluginRegistration.BaseUrl, UriKind.Absolute);
                    var jsonResult = await Dispatch(curAction, baseUri);


                    //this JSON error check is broken because it triggers on standard success messages, which look like this:
                    //"{\"success\": {\"ErrorCode\": \"0\", \"StatusCode\": \"200\", \"Description\": \"\"}}"


                    //check if the returned JSON is Error
                    //  if (jsonResult.ToLower().Contains("error"))
                    // {
                    //     curAction.ActionState = ActionState.Error;
                    //  }
                    //   else
                    //   {
                    curAction.ActionState = ActionState.Completed;
                 //   }

                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();
                }
                else
                {
                    curAction.ActionState = ActionState.Error;
                    uow.ActionRepository.Attach(curAction);
                    uow.SaveChanges();
                    throw new Exception(string.Format("Action ID: {0} status is {1}.", curAction.Id, curAction.ActionState));
                }
            }
            return curAction.ActionState.Value;
        }

        public async Task<string> Dispatch(ActionDO curActionDO, Uri curBaseUri)
        {
            PayloadMappingsDTO mappings;
            if (curActionDO == null)
                throw new ArgumentNullException("curAction");

            var curPluginClient = ObjectFactory.GetInstance<IPluginTransmitter>();
            curPluginClient.BaseUri = curBaseUri;
            var actionPayloadDTO = Mapper.Map<ActionPayloadDTO>(curActionDO);
            actionPayloadDTO.EnvelopeId = curActionDO.ParentActionList.Process.EnvelopeId; 
            
            //this is currently null because ProcessId isn't being written to ActionList.
            //that probably wasn't implemented because it doesn't actually make much sense to store a ProcessID on an ActionList
            //that's because an ActionList is essentially a template that's part of a processnodetemplate, from which N different Processes can be spawned
            //the confusion stems from design flaws that will be addressed in 921.   
            //in the short run, modify ActionList#Process to write the current processid into the ActionListDo, just to unblock this.                                                                    
            
            
            //If no existing payload, created and save it
            if (actionPayloadDTO.PayloadMappings.Count() == 0)
            {
                mappings = CreateActionPayload(curActionDO, actionPayloadDTO.EnvelopeId);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    curActionDO.PayloadMappings = mappings.Serialize();
                    uow.SaveChanges();
                }
                actionPayloadDTO.PayloadMappings = mappings;
            }

            var jsonResult = await curPluginClient.PostActionAsync(curActionDO.Name, actionPayloadDTO);
            EventManager.ActionDispatched(actionPayloadDTO);
            return jsonResult;
        }

        public PayloadMappingsDTO CreateActionPayload(ActionDO curActionDO, string curEnvelopeId)
        {
            var curEnvelopeData = _envelope.GetEnvelopeData(curEnvelopeId);
            if (String.IsNullOrEmpty(curActionDO.FieldMappingSettings))
            {
                throw new InvalidOperationException("Field mappings are empty on ActionDO with id " + curActionDO.Id);
            }
            return _envelope.ExtractPayload(curActionDO.FieldMappingSettings, curEnvelopeId, curEnvelopeData);
        }

        /// <summary>
        /// Retrieve the list of data sources for the drop down list boxes on the left side of the field mapping pane in process builder
        /// </summary>
        public IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO)
        {
            DocuSignTemplateSubscriptionDO curDocuSignSubscription = null;

            if (curActionDO.ParentActionList != null)
            {
                // Try to get ProcessTemplate.Id from relation chain
                // Action -> ActionList -> ProcessNodeTemplate -> ProcessTemplate.
                var curProcessTemplateId = curActionDO
                    .ParentActionList
                    .ProcessNodeTemplate
                    .ProcessTemplate
                    .Id;

                // Try to get DocuSignSubscription related to current ProcessTemplate.Id.
                curDocuSignSubscription = uow.ExternalEventSubscriptionRepository
                    .GetQuery()
                    .OfType<DocuSignTemplateSubscriptionDO>()
                    .FirstOrDefault(x => x.DocuSignProcessTemplateId == curProcessTemplateId);
            }

            // Return list of mappable source fields, in case we fetched DocuSignSubscription object.
            if (curDocuSignSubscription != null)
            {
                _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
                var curMappableSourceFields = _docusignTemplate
                    .GetMappableSourceFields(curDocuSignSubscription.DocuSignTemplateId);

                return curMappableSourceFields;
            }
            // Return empty list in other case.
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Retrieve the list of data sources for the text labels on the  right side of the field mapping pane in process builder.
        /// </summary>
        public Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curActionDO)
        {
            var _parentPluginRegistration = BasePluginRegistration.GetPluginType(curActionDO);
            return _parentPluginRegistration.GetFieldMappingTargets(curActionDO);
        }


		public List<ActivityDO> GetUpstreamActivities(ActionDO curActionDO)
		{
			if (curActionDO == null)
				throw new ArgumentNullException("curActionDO");
			List<ActivityDO> result = new List<ActivityDO>();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				ActionListDO curActionList = curActionDO.ParentActionList;
				while (curActionList != null)
				{
					// Get action with lower Ordering
					var lowerAction = curActionList.Actions.OrderBy(x => x.Ordering).FirstOrDefault();
					if (lowerAction != null)
						result.Add(lowerAction);
					result.Add(curActionList);
					// Go to parent ActionListDO
					curActionList = curActionList.ParentActionList;
				}
			}
			return result;
		}
		public List<ActivityDO> GetDownstreamActivities(ActionDO curActionDO)
		{
			if (curActionDO == null)
				throw new ArgumentNullException("curActionDO");
			List<ActivityDO> downstreamList = new List<ActivityDO>();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				GetDownstreamActivitiesRecursive(uow, curActionDO.ParentActionList, curActionDO.Ordering, downstreamList, new HashSet<ActionListDO>());
			}
			return downstreamList;
		}
		private void GetDownstreamActivitiesRecursive(IUnitOfWork uow, ActionListDO curActionList, int startingOrdering, List<ActivityDO> downstreamList, HashSet<ActionListDO> alreadyStartedTraversingList)
		{
			while (curActionList != null)
			{
				// Do we start traverse on this curActionList?
				if (alreadyStartedTraversingList.Contains(curActionList))
					return;
				alreadyStartedTraversingList.Add(curActionList);
				// Get the higher ordered (downstream) activities for the current ActionList
				var higherChildren = GetChildren(uow, curActionList).Where(x => x.Ordering > startingOrdering);
				// For any ActionLists, add their children
				foreach (var higherActivity in higherChildren)
				{
					downstreamList.Add(higherActivity);
					var childActionList = higherActivity as ActionListDO;
					if (childActionList != null)
						GetDownstreamActivitiesRecursive(uow, childActionList, 0, downstreamList, alreadyStartedTraversingList);
				}
				// Work up the parent ActionList chain to get activities that are downstream of the path.
				startingOrdering = curActionList.Ordering;
				curActionList = curActionList.ParentActionList;
			}
		}
		private IEnumerable<ActivityDO> GetChildren(IUnitOfWork uow, ActivityDO currActivity)
		{
			// We don't have ActivityRepository at this moment
			// To get all activities we need to get all ActionLists(filtred by ParentActionListID) and all Actions(filtred by ParentActionListID)
			var orderedActionLists = uow.ActionListRepository.GetAll()
						.Where(x => x.ParentActionListId == currActivity.Id)
						.OrderBy(z => z.Ordering);
			var orderedActions = uow.ActionRepository.GetAll()
				.Where(x => x.ParentActionListId == currActivity.Id)
				.OrderBy(z => z.Ordering);
			// We are putting all things to ActivityDO array and sorting it by Ordering property because of importance of order
			// Created needed size array
			ActivityDO[] orderedActivities = new ActivityDO[orderedActionLists.Count() + orderedActions.Count()];
			int k = 0;
			// Fill array
			foreach (var actionList in orderedActionLists)
				orderedActivities[k++] = actionList;
			foreach (var action in orderedActions)
				orderedActivities[k++] = action;
			// Sort it by property 'Ordering' because the order is important
			Array.Sort(orderedActivities, (x, y) =>
			{
				return x.Ordering.CompareTo(y.Ordering);
			});
			return orderedActivities;
		}
    }
}