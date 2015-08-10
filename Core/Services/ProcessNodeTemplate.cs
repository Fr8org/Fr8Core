using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;

namespace Core.Services
{
    public class ProcessNodeTemplate : IProcessNodeTemplate, IUnitOfWorkAwareComponent
    {
        /// <summary>
        /// Create ProcessNodeTemplate entity with required children criteria entity.
        /// </summary>
        public void Create(ProcessNodeTemplateDO processNodeTemplate)
        {
            this.InUnitOfWork(uow =>
            {
                var criteria = new CriteriaDO();
                uow.CriteriaRepository.Add(criteria);

                processNodeTemplate.Criteria = criteria;
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplate);
            });
        }

        /// <summary>
        /// Update ProcessNodeTemplate entity.
        /// </summary>
        public void Update(ProcessNodeTemplateDO processNodeTemplate)
        {
            this.InUnitOfWork(uow =>
            {
                var attachedEntity = uow.ProcessNodeTemplateRepository.GetByKey(processNodeTemplate.Id);
                if (processNodeTemplate == null)
                {
                    throw new Exception(string.Format("Unable to find criteria by id = {0}", processNodeTemplate.Id));
                }

                attachedEntity.Name = processNodeTemplate.Name;
                attachedEntity.TransitionKey = processNodeTemplate.TransitionKey;
            });
        }

        /// <summary>
        /// Remove ProcessNodeTemplate entity by id with criteria entity.
        /// </summary>
        public ProcessNodeTemplateDO Remove(int id)
        {
            return this.InUnitOfWork(uow =>
            {
                var processNodeTemplate = uow.ProcessNodeTemplateRepository
                    .GetQuery()
                    .Include("Criteria")
                    .SingleOrDefault(x => x.Id == id);

                if (processNodeTemplate == null)
                {
                    throw new Exception(string.Format("Unable to find ProcessNodeTemplate by id = {0}", id));
                }

                uow.CriteriaRepository.Remove(processNodeTemplate.Criteria);
                uow.ProcessNodeTemplateRepository.Remove(processNodeTemplate);

                return processNodeTemplate;
            });
        }
    }
}
