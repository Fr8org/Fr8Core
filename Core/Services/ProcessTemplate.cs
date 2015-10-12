using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        
        
        // private readonly IProcess _process;
        private readonly IProcessNodeTemplate _processNodeTemplate;
        private readonly DockyardAccount _dockyardAccount;
        private readonly IAction _action;
        private readonly IActivity _activity;
        private readonly ICrate _crate;

        
        

        public ProcessTemplate()
        {
            _processNodeTemplate = ObjectFactory.GetInstance<IProcessNodeTemplate>();
            _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            _action = ObjectFactory.GetInstance<IAction>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        

        public IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null, int? status = null)
        {
            if (userId == null)
                throw new ApplicationException("UserId must not be null");

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.ProcessTemplateRepository.GetQuery().Include(pt => pt.Activities); // whe have to include Activities as it is a real navigational property. Not ProcessTemplates

                if (isAdmin)
                {
                    queryableRepo = (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id));
                    return (status == null ? queryableRepo : queryableRepo.Where(pt => pt.ProcessTemplateState == status)).ToList();
                }

                queryableRepo = (id == null
                    ? queryableRepo.Where(pt => pt.DockyardAccount.Id == userId)
                    : queryableRepo.Where(pt => pt.Id == id && pt.DockyardAccount.Id == userId));
                return (status == null
                    ? queryableRepo : queryableRepo.Where(pt => pt.ProcessTemplateState == status)).ToList();
            }
        }

        

        public void CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo, bool updateChildEntities)
        {
            var creating = ptdo.Id == 0;
            if (creating)
            {
                ptdo.ProcessTemplateState = ProcessTemplateState.Inactive;
                var processNodeTemplate = new ProcessNodeTemplateDO(true);
                processNodeTemplate.ParentActivity = ptdo;
                ptdo.Activities.Add(processNodeTemplate);

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
            var curProcessTemplate = uow.ProcessTemplateRepository.GetQuery().SingleOrDefault(pt => pt.Id == id);

            if (curProcessTemplate == null)
            {
                throw new EntityNotFoundException<ProcessTemplateDO>(id);
            }

            _activity.Delete(uow, curProcessTemplate);
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

        

        private IEnumerable<TActivity> EnumerateActivities<TActivity>(ProcessTemplateDO curProcessTemplate, bool allowOnlyOneNoteTemplate = true)
        {
            bool firstNodeTemplate = true;

            foreach (ProcessNodeTemplateDO template in curProcessTemplate.ProcessNodeTemplates)
            {
                if (allowOnlyOneNoteTemplate && !firstNodeTemplate)
                {
                    throw new Exception("More than one process node template with non empty list of action exsists in the process");
                }

                firstNodeTemplate = false;

                if (template.Activities != null)
                {
                    foreach (var activityDo in template.Activities.OfType<TActivity>())
                    {
                        yield return activityDo;
                    }
                }
            }
        }

        

        public string Activate(ProcessTemplateDO curProcessTemplate)
        {
            if (curProcessTemplate.ProcessNodeTemplates == null)
            {
                throw new ArgumentNullException("Parameter ProcessNodeTemplates is null.");
            }

            string result = "no action";

            foreach (var curActionDO in EnumerateActivities<ActionDO>(curProcessTemplate))
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

            return result;
        }

        

        public string Deactivate(ProcessTemplateDO curProcessTemplate)
        {
            string result = "no action";

            foreach (var curActionDO in EnumerateActivities<ActionDO>(curProcessTemplate))
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

            return result;
        }

        
        // TODO: like some other methods, this assumes that there is only 1 action list in use. This is dangerous 
        //because the database allows N Activities.
        //we're waiting to reconcile this until we get some visibility into how the product is used by users
//        public ActionListDO GetActionList(IUnitOfWork uow, int id)
//        {
//            // Get action list by process template first 
//            var currentProcessTemplate = uow.ProcessTemplateRepository.GetQuery().Where(pt => pt.Id == id).ToArray();
//
//            if (currentProcessTemplate.Length == 0)
//            {
//                return null;
//            }
//
//            if (currentProcessTemplate.Length > 1)
//            {
//                throw new Exception(string.Format("More than one action list exists in processtemplate {0}", id));
//            }
//
//            var startingProcessTemplate = currentProcessTemplate[0].StartingProcessNodeTemplate;
//            if (startingProcessTemplate == null)
//            {
//                return null;
//            }
//
//            // Get Activities related to the ProcessTemplate
//            var curActionList = startingProcessTemplate.Activities.SingleOrDefault(al => al.ActionListType == ActionListType.Immediate);
//            return curActionList;
//
//        }

        
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
                return EnumerateActivities<ActionDO>(uow.ProcessTemplateRepository.GetByKey(id), false).ToArray();
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
                        EventSubscriptionMS subscriptionsList = _crate.GetContents<EventSubscriptionMS>(curEventSubscription);

                        bool hasEvents = subscriptionsList.Subscriptions.Any(events => curEventReport.EventNames.ToUpper().Trim().Contains(events.ToUpper()));

                        if (hasEvents)
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
                return EnumerateActivities<ActivityDO>(uow.ProcessTemplateRepository.GetByKey(curProcessTemplateId)).FirstOrDefault();
            }
        }

        

        public ActivityDO GetInitialActivity(IUnitOfWork uow, ProcessTemplateDO curProcessTemplate)
        {
            return EnumerateActivities<ActivityDO>(curProcessTemplate).OrderBy(a => a.Ordering).FirstOrDefault();
        }

        public ProcessTemplateDO GetProcessTemplate(ActionDO action)
        {
            var root = action.ParentActivity;

            while (root != null)
            {
                if (root is ProcessTemplateDO)
                {
                    return (ProcessTemplateDO)root;
                }

                root = root.ParentActivity;
            }

            return null;
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