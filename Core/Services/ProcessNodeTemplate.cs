using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;

namespace Core.Services
{
    public class ProcessNodeTemplate : IProcessNodeTemplate
    {
        /// <summary>
        /// Create ProcessNodeTemplate entity with required children criteria entity.
        /// </summary>
        public void Create(IUnitOfWork uow, ProcessNodeTemplateDO processNodeTemplate)
        {
            if (processNodeTemplate == null)
            {
                throw new Exception("Creating logic was passed a null ProcessNodeTemplateDO");
            }
            uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);

            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                ProcessNodeTemplate = processNodeTemplate,
                ExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);

            // Saving immediate action list entity in repository.
            var immediateActionList = new ActionListDO()
            {
                Name = "Immediate",
                ActionListType = ActionListType.Immediate,
                ProcessNodeTemplate = processNodeTemplate
            };
            uow.ActionListRepository.Add(immediateActionList);

            // Saving scheduled action list entity in repository.
            var scheduledActionList = new ActionListDO()
            {
                Name = "Scheduled",
                ActionListType = ActionListType.Scheduled,
                ProcessNodeTemplate = processNodeTemplate
            };
            uow.ActionListRepository.Add(scheduledActionList);
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
            curProcessNodeTemplate.TransitionKey = processNodeTemplate.TransitionKey;
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
            uow.ActionRepository
                .GetQuery()
                .Where(x => x.ActionList.ProcessNodeTemplate.Id == id)
                .ToList()
                .ForEach(x => uow.ActionRepository.Remove(x));

            uow.SaveChanges();

            // Remove all action-lists.
            uow.ActionListRepository
                .GetQuery()
                .Where(x => x.ProcessNodeTemplateID == id)
                .ToList()
                .ForEach(x => uow.ActionListRepository.Remove(x));

            uow.SaveChanges();

            // Remove Criteria.
            uow.CriteriaRepository
                .GetQuery()
                .Where(x => x.ProcessNodeTemplateID == id)
                .ToList()
                .ForEach(x => uow.CriteriaRepository.Remove(x));

            uow.SaveChanges();

            // Remove ProcessNodeTemplate.
            uow.ProcessNodeTemplateRepository.Remove(processNodeTemplate);
            uow.SaveChanges();
        }
    }
}
