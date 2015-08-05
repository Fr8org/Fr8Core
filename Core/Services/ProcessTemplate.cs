using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        public IList<ProcessTemplateDO> GetForUser(string userId, int? id = null)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var predicate = id != null
                    ? (Func<ProcessTemplateDO, bool>) (pt => pt.Id == id)
                    : (pt => pt.UserId == userId);


                return unitOfWork.ProcessTemplateRepository
                .GetQuery()
                .Where(predicate).ToList();
            }
        }


        public int CreateOrUpdate(ProcessTemplateDO ptdo)
        {
            var creating = ptdo.Id == 0;

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (creating)
                {
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
    }
}
