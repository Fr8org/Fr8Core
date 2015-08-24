using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        private readonly IProcess _process;

        public ProcessTemplate()
        {
            _process = ObjectFactory.GetInstance<IProcess>();
        }

        public IList<ProcessTemplateDO> GetForUser(string userId, bool isAdmin = false, int? id = null)
        {
            if (userId == null)
                throw new ApplicationException("UserId must not be null");

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var queryableRepo = unitOfWork.ProcessTemplateRepository.GetQuery();

                if (isAdmin)
                {
                    return (id == null ? queryableRepo : queryableRepo.Where(pt => pt.Id == id)).ToList();
                }

                return (id == null
                    ? queryableRepo.Where(pt => pt.DockyardAccount.Id == userId)
                    : queryableRepo.Where(pt => pt.Id == id && pt.DockyardAccount.Id == userId)).ToList();
            }
        }

        public int CreateOrUpdate(IUnitOfWork uow, ProcessTemplateDO ptdo)
        {
            var creating = ptdo.Id == 0;
            try
            {
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
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            uow.SaveChanges();

            return ptdo.Id;
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

        public void LaunchProcess(IUnitOfWork uow, ProcessTemplateDO curProcessTemplate, EnvelopeDO curEnvelope)
        {
            if (curProcessTemplate == null)
                throw new EntityNotFoundException(curProcessTemplate);

            if (curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive)
            {
                _process.Launch(curProcessTemplate, curEnvelope);
                ProcessDO launchedProcess = uow.ProcessRepository.FindOne(
                    process =>
                        process.Name.Equals(curProcessTemplate.Name) && process.EnvelopeId.Equals(curEnvelope.Id.ToString()) &&
                        process.ProcessState == ProcessState.Executing);
                EventManager.ProcessLaunched(launchedProcess);
            }
        }
    }
}