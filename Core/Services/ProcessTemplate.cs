using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
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
                    ? queryableRepo.Where(pt => pt.UserId == userId)
                    : queryableRepo.Where(pt => pt.Id == id && pt.UserId == userId)).ToList();
            }
        }

        public int CreateOrUpdate(ProcessTemplateDO ptdo)
        {
            var creating = ptdo.Id == 0;

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (creating)
                {
                    ptdo.ProcessTemplateState = ProcessTemplateState.Inactive;
                    unitOfWork.ProcessTemplateRepository.Add(ptdo);
                }
                else
                {
                    var curProcessTemplate = unitOfWork.ProcessTemplateRepository.GetByKey(ptdo.Id);
                    if (curProcessTemplate == null)
                        throw new EntityNotFoundException();
                    curProcessTemplate.Name = ptdo.Name;
                    curProcessTemplate.Description = ptdo.Description;
                }
                unitOfWork.SaveChanges();
            }

            return ptdo.Id;
        }

        public void Delete(int id)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplate = unitOfWork.ProcessTemplateRepository.GetByKey(id);
                if (curProcessTemplate == null)
                {
                    throw new EntityNotFoundException<ProcessTemplateDO>(id);
                }
                unitOfWork.ProcessTemplateRepository.Remove(curProcessTemplate);
                unitOfWork.SaveChanges();
            }
        }

        public void LaunchProcess(int curProcessTemplateId, EnvelopeDO curEnvelope)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplate = uow.ProcessTemplateRepository.GetByKey(curProcessTemplateId);
                if (curProcessTemplate == null)
                    throw new EntityNotFoundException(curProcessTemplateId);

                if (curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive)
                {
                    _process.Execute(curProcessTemplate, curEnvelope);
                }
            }
        }
    }
}