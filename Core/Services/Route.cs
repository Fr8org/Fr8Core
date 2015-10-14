using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;

namespace Core.Services
{
    public class Route : IRoute
    {


        // private readonly IProcess _process;
        private readonly ISubroute _subroute;
        private readonly DockyardAccount _dockyardAccount;
        private readonly IAction _action;
        private readonly IActivity _activity;
        private readonly ICrate _crate;

        public Route()
        {
            _subroute = ObjectFactory.GetInstance<ISubroute>();
            _dockyardAccount = ObjectFactory.GetInstance<DockyardAccount>();
            _action = ObjectFactory.GetInstance<IAction>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        public IList<RouteDO> GetForUser(IUnitOfWork unitOfWork, DockyardAccountDO account, bool isAdmin = false, int? id = null, int? status = null)
        {
            var queryableRepo = unitOfWork.RouteRepository.GetQuery().Include(pt => pt.Activities); // whe have to include Activities as it is a real navigational property. Not Routes

            if (isAdmin)
            {
                queryableRepo = (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id));
                return (status == null ? queryableRepo : queryableRepo.Where(pt => pt.RouteState == status)).ToList();
            }

            queryableRepo = (id == null
                ? queryableRepo.Where(pt => pt.DockyardAccount.Id == account.Id)
                : queryableRepo.Where(pt => pt.Id == id && pt.DockyardAccount.Id == account.Id));

            return (status == null
                ? queryableRepo : queryableRepo.Where(pt => pt.RouteState == status)).ToList();

        }
        
        public void CreateOrUpdate(IUnitOfWork uow, RouteDO ptdo, bool updateChildEntities)
        {
            var creating = ptdo.Id == 0;
            if (creating)
            {
                ptdo.RouteState = RouteState.Inactive;
                var subroute = new SubrouteDO(true);
                subroute.ParentActivity = ptdo;
                ptdo.Activities.Add(subroute);

                uow.RouteRepository.Add(ptdo);
                _subroute.Create(uow, ptdo.StartingSubroute);
            }
            else
            {
                var curRoute = uow.RouteRepository.GetByKey(ptdo.Id);
                if (curRoute == null)
                    throw new EntityNotFoundException();
                curRoute.Name = ptdo.Name;
                curRoute.Description = ptdo.Description;
                // ChildEntities update code has been deleted by demel 09/28/2015
            }
            //uow.SaveChanges(); we don't want to save changes here. we want the calling method to get to decide when this uow should be saved as a group
            // return ptdo.Id;
        }



        public void Delete(IUnitOfWork uow, int id)
        {
            var curRoute = uow.RouteRepository.GetQuery().SingleOrDefault(pt => pt.Id == id);

            if (curRoute == null)
            {
                throw new EntityNotFoundException<RouteDO>(id);
            }

            _activity.Delete(uow, curRoute);
        }



        public IList<SubrouteDO> GetSubroutes(RouteDO curRouteDO)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.RouteRepository.GetQuery()
                    .Include("Subroutes")
                    .Where(x => x.Id == curRouteDO.Id);

                return queryableRepo.SelectMany<RouteDO, SubrouteDO>(x => x.Subroutes)
                    .ToList();
            }
        }



        private IEnumerable<TActivity> EnumerateActivities<TActivity>(RouteDO curRoute, bool allowOnlyOneNoteTemplate = true)
        {
            bool firstNodeTemplate = true;

            foreach (SubrouteDO template in curRoute.Subroutes)
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



        public string Activate(RouteDO curRoute)
        {
            if (curRoute.Subroutes == null)
            {
                throw new ArgumentNullException("Parameter Subroutes is null.");
            }

            string result = "no action";

            foreach (var curActionDO in EnumerateActivities<ActionDO>(curRoute))
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



        public string Deactivate(RouteDO curRoute)
        {
            string result = "no action";

            foreach (var curActionDO in EnumerateActivities<ActionDO>(curRoute))
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
        //            var currentRoute = uow.RouteRepository.GetQuery().Where(pt => pt.Id == id).ToArray();
        //
        //            if (currentRoute.Length == 0)
        //            {
        //                return null;
        //            }
        //
        //            if (currentRoute.Length > 1)
        //            {
        //                throw new Exception(string.Format("More than one action list exists in processtemplate {0}", id));
        //            }
        //
        //            var startingRoute = currentRoute[0].StartingSubroute;
        //            if (startingRoute == null)
        //            {
        //                return null;
        //            }
        //
        //            // Get Activities related to the Route
        //            var curActionList = startingRoute.Activities.SingleOrDefault(al => al.ActionListType == ActionListType.Immediate);
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
                return EnumerateActivities<ActionDO>(uow.RouteRepository.GetByKey(id), false).ToArray();
            }
        }



        public IList<RouteDO> GetMatchingRoutes(string userId, EventReportCM curEventReport)
        {
            List<RouteDO> processTemplateSubscribers = new List<RouteDO>();
            if (String.IsNullOrEmpty(userId))
                throw new ArgumentNullException("Parameter UserId is null");
            if (curEventReport == null)
                throw new ArgumentNullException("Parameter Standard Event Report is null");

            //1. Query all RouteDO that are Active
            //2. are associated with the determined DockyardAccount
            //3. their first Activity has a Crate of  Class "Standard Event Subscriptions" which has inside of it an event name that matches the event name 
            //in the Crate of Class "Standard Event Reports" which was passed in.
            var curRoutes = _dockyardAccount.GetActiveRoutes(userId).ToList();

            return MatchEvents(curRoutes, curEventReport);
            //3. Get ActivityDO

        }



        public List<RouteDO> MatchEvents(List<RouteDO> curRoutes,
            EventReportCM curEventReport)
        {
            List<RouteDO> subscribingRoutes = new List<RouteDO>();
            foreach (var curRoute in curRoutes)
            {
                //get the 1st activity
                var actionDO = GetFirstActivity(curRoute.Id) as ActionDO;

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
                        //Parse CrateDTO to EventReportMS and compare Event name then add the Route to the results
                        EventSubscriptionCM subscriptionsList = _crate.GetContents<EventSubscriptionCM>(curEventSubscription);

                        bool hasEvents = subscriptionsList.Subscriptions
                            .Where(events => curEventReport.EventNames.ToUpper().Trim().Replace(" ", "").Contains(events.ToUpper().Trim().Replace(" ", "")))
                            .Any();

                        if (hasEvents)
                        {
                            subscribingRoutes.Add(curRoute);
                        }
                    }
                }
            }
            return subscribingRoutes;

        }



        public ActivityDO GetFirstActivity(int curRouteId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return EnumerateActivities<ActivityDO>(uow.RouteRepository.GetByKey(curRouteId)).FirstOrDefault();
            }
        }



        public ActivityDO GetInitialActivity(IUnitOfWork uow, RouteDO curRoute)
        {
            return EnumerateActivities<ActivityDO>(curRoute).OrderBy(a => a.Ordering).FirstOrDefault();
        }

        public RouteDO GetRoute(ActionDO action)
        {
            var root = action.ParentActivity;

            while (root != null)
            {
                if (root is RouteDO)
                {
                    return (RouteDO)root;
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