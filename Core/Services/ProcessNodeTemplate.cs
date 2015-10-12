using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class ProcessNodeTemplate : IProcessNodeTemplate
    {
        /// <summary>
        /// Create ProcessNodeTemplate entity with required children criteria entity.
        /// </summary>
        public void Create(IUnitOfWork uow, ProcessNodeTemplateDO processNodeTemplate )
        {
            if (processNodeTemplate == null)
            {
                processNodeTemplate = ObjectFactory.GetInstance<ProcessNodeTemplateDO>();
            }

            uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
            
            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                ProcessNodeTemplate = processNodeTemplate,
                CriteriaExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);
            
            //we don't want to save changes here, to enable upstream transactions
        }

        /// <summary>
        /// Update ProcessNodeTemplate entity.
        /// </summary>
        public void Update(IUnitOfWork uow, ProcessNodeTemplateDO processNodeTemplate)
        {
            if (processNodeTemplate == null)
            {
                throw new Exception("Updating logic was passed a null ProcessNodeTemplateDO");
            }

            var curProcessNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(processNodeTemplate.Id);
            if (curProcessNodeTemplate == null)
            {
                throw new Exception(string.Format("Unable to find criteria by id = {0}", processNodeTemplate.Id));
            }

            curProcessNodeTemplate.Name = processNodeTemplate.Name;
            curProcessNodeTemplate.NodeTransitions = processNodeTemplate.NodeTransitions;
            uow.SaveChanges();
        }

        /// <summary>
        /// Remove ProcessNodeTemplate and children entities by id.
        /// </summary>
        public void Delete(IUnitOfWork uow, int id)
        {
            var processNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(id);

            if (processNodeTemplate == null)
            {
                throw new Exception(string.Format("Unable to find ProcessNodeTemplate by id = {0}", id));
            }

            // Remove all actions.

            

          //  processNodeTemplate.Activities.ForEach(x => uow.ActivityRepository.Remove(x));

//            uow.SaveChanges();
//            
//            // Remove Criteria.
//            uow.CriteriaRepository
//                .GetQuery()
//                .Where(x => x.ProcessNodeTemplateId == id)
//                .ToList()
//                .ForEach(x => uow.CriteriaRepository.Remove(x));
//
//            uow.SaveChanges();

            // Remove ProcessNodeTemplate.
            //uow.ProcessNodeTemplateRepository.Remove(processNodeTemplate);


            ObjectFactory.GetInstance<IActivity>().Delete(uow, processNodeTemplate);

            uow.SaveChanges();
        }

        public void AddAction(IUnitOfWork uow, ActionDO curActionDO)
        {
            var processNodeTemplate = uow.ProcessNodeTemplateRepository.GetByKey(curActionDO.ParentActivityId);

            if (processNodeTemplate == null)
            {
                throw new Exception(string.Format("Unable to find ProcessNodeTemplate by id = {0}", curActionDO.ParentActivityId));
            }

            curActionDO.Ordering = processNodeTemplate.Activities.Count > 0 ? processNodeTemplate.Activities.Max(x => x.Ordering) + 1 : 1;

            processNodeTemplate.Activities.Add(curActionDO);

            uow.SaveChanges();
        }
    }
}
