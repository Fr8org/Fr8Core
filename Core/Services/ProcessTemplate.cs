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
using Newtonsoft.Json;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        private readonly IProcess _process;
        private IAction _action;

        public object itemsToRemove { get; private set; }

        public ProcessTemplate()
        {
            _process = ObjectFactory.GetInstance<IProcess>();
            _action = ObjectFactory.GetInstance<IAction>();
        }

        public IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null)
        {
            if (userId == null)
                throw new ApplicationException("UserId must not be null");

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.ProcessTemplateRepository.GetQuery()
                    .Include("SubscribedDocuSignTemplates")
                    .Include("SubscribedExternalEvents");

                if (isAdmin)
                {
                    return (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id)).ToList();
                }

                return (id == null
                    ? queryableRepo.Where(pt => pt.DockyardAccount.Id == userId)
                    : queryableRepo.Where(pt => pt.Id == id && pt.DockyardAccount.Id == userId)).ToList();
            }
        }

        public int CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo, bool updateChildEntities)
        {
            var creating = ptdo.Id == 0;
            if (creating)
            {
                ptdo.ProcessTemplateState = ProcessTemplateState.Inactive;
                uow.ProcessTemplateRepository.Add(ptdo);
            }
            else
            {
                var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(ptdo.Id);
                if (curProcessTemplate == null)
                    throw new EntityNotFoundException();
                curProcessTemplate.Name = ptdo.Name;
                curProcessTemplate.Description = ptdo.Description;

                //
                if (updateChildEntities)
                {
                    //Update DocuSign template registration
                    MakeCollectionEqual(uow, curProcessTemplate.SubscribedDocuSignTemplates,
                        ptdo.SubscribedDocuSignTemplates);

                    //Update DocuSign trigger registration
                    MakeCollectionEqual(uow, curProcessTemplate.SubscribedExternalEvents,
                        ptdo.SubscribedExternalEvents);
                }
            }
            uow.SaveChanges();
            return ptdo.Id;
        }

        /// <summary>
        /// The function add/removes items on the current collection 
        /// so that they match the items on the new collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionToUpdate"></param>
        /// <param name="sourceCollection"></param>
        public void MakeCollectionEqual<T>(IUnitOfWork uow, IList<T> collectionToUpdate, IList<T> sourceCollection) where T : class
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

        public void Delete(IUnitOfWork uow, int id)
        {
            var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(id);
            if (curProcessTemplate == null)
            {
                throw new EntityNotFoundException<ProcessTemplateDO>(id);
            }
            uow.ProcessTemplateRepository.Remove(curProcessTemplate);
        }

        public void LaunchProcess(IUnitOfWork uow, ProcessTemplateDO curProcessTemplate, CrateDTO curEventData)
        {
            if (curProcessTemplate == null)
                throw new EntityNotFoundException(curProcessTemplate);

            if (curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive)
            {
                _process.Launch(curProcessTemplate, curEventData);

                //todo: what does this do?
                //ProcessDO launchedProcess = uow.ProcessRepository.FindOne(
                //    process =>
                //        process.Name.Equals(curProcessTemplate.Name) && process.EnvelopeId.Equals(envelopeIdField.Value) &&
                //        process.ProcessState == ProcessState.Executing);
                //EventManager.ProcessLaunched(launchedProcess);
            }
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
            //List<ActionDO> curActions = (List<ActionDO>)curProcessTemplate.ProcessNodeTemplates[0].ActionLists[0].Activities.Where(p => p.ParentActivityId == p.Id);
            // var curActionLists = curProcessTemplate.ProcessNodeTemplates.Select(pt => pt.ActionLists);
            //var cuuActivity = curActionLists.Select(al => al.Select(a => a.Activities));
            try
            {
                foreach (ProcessNodeTemplateDO processNodeTemplates in curProcessTemplate.ProcessNodeTemplates)
                {
                    foreach (ActionListDO actions in processNodeTemplates.ActionLists)
                    {
                        foreach (var item in actions.Activities)
                        {
                           
                            _action.Activate((ActionDO)item);
                        }
                    }
                }
                return "success";
            }
            catch (Exception) { return "fail"; }
        }

        public string Deactivate(ProcessTemplateDO curProcessTemplate)
        {
            try
            {
                foreach (ProcessNodeTemplateDO processNodeTemplates in curProcessTemplate.ProcessNodeTemplates)
                {
                    foreach (ActionListDO actions in processNodeTemplates.ActionLists)
                    {
                        foreach (var item in actions.Activities)
                        {
                            _action.Deactivate((ActionDO)item);
                        }
                    }
                }
                return "success";
            }
            catch (Exception) { return "fail"; }
        }
    }
}