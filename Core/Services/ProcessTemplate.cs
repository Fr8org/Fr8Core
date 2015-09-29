using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using System.Data.Entity;
using StructureMap;
using System.Data;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        // private readonly IProcess _process;
        private readonly IProcessNodeTemplate _processNodeTemplate;
        private readonly DockyardAccount _dockyardAccount;
        private readonly IAction _action;
        private readonly ICrate _crate;

        public object itemsToRemove { get; private set; }

        public ProcessTemplate()
        {
            _processNodeTemplate = ObjectFactory.GetInstance<IProcessNodeTemplate>();
            _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            _action = ObjectFactory.GetInstance<IAction>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        public IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null)
        {
            if (userId == null)
                throw new ApplicationException("UserId must not be null");

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.ProcessTemplateRepository.GetQuery().Include(pt => pt.ProcessNodeTemplates);

                if (isAdmin)
                {
                    return (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id)).ToList();
                }

                return (id == null
                    ? queryableRepo.Where(pt => pt.DockyardAccount.Id == userId)
                    : queryableRepo.Where(pt => pt.Id == id && pt.DockyardAccount.Id == userId)).ToList();
            }
        }

        public void CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo, bool updateChildEntities)
        {
            var creating = ptdo.Id == 0;
            if (creating)
            {
                ptdo.ProcessTemplateState = ProcessTemplateState.Inactive;
                var processNodeTemplate = new ProcessNodeTemplateDO(true);
                processNodeTemplate.ProcessTemplate = ptdo;
                ptdo.ProcessNodeTemplates.Add(processNodeTemplate);

                uow.ProcessTemplateRepository.Add(ptdo);
                _processNodeTemplate.Create(uow, ptdo.StartingProcessNodeTemplate);
            }
            else
            {
                var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(ptdo.Id);
                if (curProcessTemplate == null)
                    throw new EntityNotFoundException();
                curProcessTemplate.Name = ptdo.Name;
                curProcessTemplate.Description = ptdo.Description;
                // ChildEntities update code has been deleted by demel 09/28/2015
            }
            //uow.SaveChanges(); we don't want to save changes here. we want the calling method to get to decide when this uow should be saved as a group
            // return ptdo.Id;
        }



        public void Delete(IUnitOfWork uow, int id)
        {
            var curProcessTemplate = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.Id == id).SingleOrDefault();
            
            if (curProcessTemplate == null)
            {
                throw new EntityNotFoundException<ProcessTemplateDO>(id);
            }

            foreach (var nodeTemplate in curProcessTemplate.ProcessNodeTemplates)
            {
                foreach (var actionList in nodeTemplate.ActionLists)
                {
                    foreach (var activity in actionList.Activities)
                    {
                        uow.ActivityRepository.Remove(activity);
                    }
                }
            }
            uow.ProcessTemplateRepository.Remove(curProcessTemplate);
        }



        public IList<ProcessNodeTemplateDO> GetProcessNodeTemplates(ProcessTemplateDO curProcessTemplateDO)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.ProcessTemplateRepository.GetQuery()
                    .Include("ProcessNodeTemplates")
                    .Where(x => x.Id == curProcessTemplateDO.Id);

                return queryableRepo.SelectMany<ProcessTemplateDO, ProcessNodeTemplateDO>(x => x.ProcessNodeTemplates)
                    .ToList();
            }
        }


        public string Activate(ProcessTemplateDO curProcessTemplate)
        {
            string result = "no action";
            foreach (ProcessNodeTemplateDO processNodeTemplates in curProcessTemplate.ProcessNodeTemplates)
            {
                foreach (
                    ActionListDO curActionList in
                        processNodeTemplates.ActionLists.Where(p => p.ParentActivityId == p.Id))
                {
                    foreach (ActionDO curActionDO in curActionList.Activities)
                    {
                        try
                        {
                            _action.Activate(curActionDO).Wait();
                            curActionDO.ActionState = ActionState.Active;
                            result = "success";
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException("Process template activation failed.", ex);
                        }
                    }
                }
            }
            return result;
        }

        public string Deactivate(ProcessTemplateDO curProcessTemplate)
        {
            string result = "no action";
            foreach (ProcessNodeTemplateDO processNodeTemplates in curProcessTemplate.ProcessNodeTemplates)
            {
                foreach (ActionListDO curActionList in processNodeTemplates.ActionLists)
                {
                    foreach (ActionDO curActionDO in curActionList.Activities)
                    {
                        try
                        {
                            _action.Deactivate(curActionDO).Wait();
                            curActionDO.ActionState = ActionState.Deactive;
                            result = "success";
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException("Process template Deactivation failed.", ex);
                        }
                    }
                }
            }
            return result;
        }

        //like some other methods, this assumes that there is only 1 action list in use. This is dangerous 
        //because the database allows N ActionLists.
        //we're waiting to reconcile this until we get some visibility into how the product is used by users
        public ActionListDO GetActionList(IUnitOfWork uow, int id)
        {
            ActionListDO curActionList = null;
         
                // Get action list by process template first 
                var curProcessTemplateQuery = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.Id == id).
                    Include(pt => pt.StartingProcessNodeTemplate.ActionLists);

                if (curProcessTemplateQuery.Count() == 0
                    || curProcessTemplateQuery.SingleOrDefault().StartingProcessNodeTemplate == null)
                    return null;

                // Get ActionLists related to the ProcessTemplate
                curActionList = curProcessTemplateQuery.SingleOrDefault()
                    .ProcessNodeTemplates.FirstOrDefault().ActionLists
                    .SingleOrDefault(al => al.ActionListType == ActionListType.Immediate);


          
            return curActionList;

        }


        /// <summary>
        /// Returns all actions created within a Process Template.
        /// </summary>
        /// <param name="id">Process Template id.</param>
        /// <returns></returns>
        public IEnumerable<ActionDO> GetActions(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("id");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emptyResult = new List<ActionDO>();

                var curActionList = GetActionList(uow,id);

                // Get all the actions for that action list
                var curActivities = uow.ActionRepository.GetAll().Where(a => a.ParentActivityId == curActionList.Id);

                if (curActivities.Count() == 0)
                    return emptyResult;

                return curActivities;
            }
        }

        public IList<ProcessTemplateDO> GetMatchingProcessTemplates(string userId, EventReportMS curEventReport)
        {
            List<ProcessTemplateDO> processTemplateSubscribers = new List<ProcessTemplateDO>();
            if (String.IsNullOrEmpty(userId))
                throw new ArgumentNullException("Parameter UserId is null");
            if (curEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null");

            //1. Query all ProcessTemplateDO that are Active
            //2. are associated with the determined DockyardAccount
            //3. their first Activity has a Crate of  Class "Standard Event Subscriptions" which has inside of it an event name that matches the event name 
            //in the Crate of Class "Standard Event Reports" which was passed in.
            var curProcessTemplates = _dockyardAccount.GetActiveProcessTemplates(userId).ToList();

            return MatchEvents(curProcessTemplates, curEventReport);
            //3. Get ActivityDO

        }

        public List<ProcessTemplateDO> MatchEvents(List<ProcessTemplateDO> curProcessTemplates,
            EventReportMS curEventReport)
        {
            List<ProcessTemplateDO> subscribingProcessTemplates = new List<ProcessTemplateDO>();
            foreach (var curProcessTemplate in curProcessTemplates)
            {
                //get the 1st activity
                var actionDO = GetFirstActivity(curProcessTemplate.Id) as ActionDO;

                //Get the CrateStorage
                if (actionDO != null && !string.IsNullOrEmpty(actionDO.CrateStorage))
                {
                    // Loop each CrateDTO in CrateStorage
                    IEnumerable<CrateDTO> eventSubscriptionCrates = _action
                        .GetCratesByManifestType(
                            CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME,
                            actionDO.CrateStorageDTO()
                        );

                    foreach (var curEventSubscription in eventSubscriptionCrates)
                    {
                        //Parse CrateDTO to EventReportMS and compare Event name then add the ProcessTemplate to the results
                        EventSubscriptionMS subscriptionsList =
                            _crate.GetContents<EventSubscriptionMS>(curEventSubscription);

                        bool hasEvents = subscriptionsList.Subscriptions
                            .Where(events => curEventReport.EventNames.ToUpper().Trim().Contains(events.ToUpper()))
                            .Any();

                        if (subscriptionsList != null && hasEvents)
                        {
                            subscribingProcessTemplates.Add(curProcessTemplate);
                        }
                    }
                }
            }
            return subscribingProcessTemplates;

        }

        public ActivityDO GetFirstActivity(int curProcessTemplateId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplateDO = uow.ProcessTemplateRepository.GetByKey(curProcessTemplateId);

                ActivityDO activityDO = null;
                var actionLists = curProcessTemplateDO.ProcessNodeTemplates
                    .SelectMany(s => s.ActionLists.Where(x => x.ActionListType == ActionListType.Immediate))
                    .ToList();

                if (actionLists.Count > 1)
                {
                    throw new Exception("Multiple ActionList found in ProcessTemplateDO");
                }

                else if (actionLists.Count > 0)
                {
                    var actionListDO = actionLists[0];
                    activityDO = actionListDO.Activities
                        .OrderBy(o => o.Ordering)
                        .FirstOrDefault();
                }

                return activityDO;
            }
        }

        public ActivityDO GetInitialActivity(ProcessTemplateDO curProcessTemplate)
        {
            ActivityDO initialActivity = null;
            //at create time, find the lowest ordered activity in the immediate Action list and set that as the current activity.
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionListDO curActionList = GetActionList(uow, curProcessTemplate.Id);


                // find all sibling actions that have a lower Ordering. These are the ones that are "above" this action in the list
                return curActionList.Activities.OrderBy(a => a.Ordering).FirstOrDefault();
            }



        }


        /// <summary>
        /// The function add/removes items on the current collection 
        /// so that they match the items on the new collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionToUpdate"></param>
        /// <param name="sourceCollection"></param>
        /* public void MakeCollectionEqual<T>(IUnitOfWork uow, IList<T> collectionToUpdate, IList<T> sourceCollection) where T : class
        {
            List<T> itemsToAdd = new List<T>();
            List<T> itemsToRemove = new List<T>();
            bool found;

            foreach (T entity in collectionToUpdate)
            {
                found = false;
                foreach (T entityToCompare in sourceCollection)
                {
                    if (((IEquatable<T>)entity).Equals(entityToCompare))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    itemsToRemove.Add(entity);
                }
            }
            itemsToRemove.ForEach(e => uow.Db.Entry(e).State = EntityState.Deleted);

            foreach (T entity in sourceCollection)
            {
                found = false;
                foreach (T entityToCompare in collectionToUpdate)
                {
                    if (((IEquatable<T>)entity).Equals(entityToCompare))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    itemsToAdd.Add(entity);
                }
            }
            itemsToAdd.ForEach(i => collectionToUpdate.Add(i));


            //identify deleted items and remove them from the collection
            //collectionToUpdate.Except(sourceCollection).ToList().ForEach(s => collectionToUpdate.Remove(s));
            //identify added items and add them to the collection
            //sourceCollection.Except(collectionToUpdate).ToList().ForEach(s => collectionToUpdate.Add(s));
        }
        */
    }
}