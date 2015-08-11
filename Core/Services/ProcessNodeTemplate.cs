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
    public class ProcessNodeTemplate : IProcessNodeTemplate, IUnitOfWorkAwareComponent
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

            var criteria = new CriteriaDO();
            uow.CriteriaRepository.Add(criteria);

            var immediateActionList = new ActionListDO()
            {
                Name = "Immediate",
                ActionListType = ActionListType.Immediate
            };

            var scheduledActionList = new ActionListDO()
            {
                Name = "Scheduled",
                ActionListType = ActionListType.Scheduled
            };

            processNodeTemplate.Criteria = criteria;
            processNodeTemplate.ActionLists.Add(immediateActionList);
            processNodeTemplate.ActionLists.Add(scheduledActionList);

            uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
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
            var processNodeTemplate = uow.ProcessNodeTemplateRepository
                .GetQuery()
                .Include(x => x.Criteria)
                .SingleOrDefault(x => x.Id == id);

            if (processNodeTemplate == null)
            {
                throw new Exception(string.Format("Unable to find ProcessNodeTemplate by id = {0}", id));
            }

            uow.CriteriaRepository.Remove(processNodeTemplate.Criteria);
            uow.ProcessNodeTemplateRepository.Remove(processNodeTemplate);
        }
    }
}
