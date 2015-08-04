using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Helper;
using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Infrastructure;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        public IQueryable<ProcessTemplateDO> GetForUser(string userId, int? id = null)
        {
            return this.Using(unitOfWork =>
            {
                return unitOfWork.ProcessTemplateRepository
                .GetQuery()
                .Where(pt => pt.UserId == userId || (id != null && pt.Id == id));
            });
        }


        public int CreateOrUpdate(ProcessTemplateDO ptdo)
        {
            var creating = ptdo.Id == 0;

            this.Using(unitOfWork =>
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
            });

            return ptdo.Id;
        }

        public void Delete(int id)
        {
            this.Using(unitOfWork =>
            {
                var curProcessTemplate = unitOfWork.ProcessTemplateRepository.GetByKey(id);
                if (curProcessTemplate == null)
                {
                    throw new EntityNotFoundException<ProcessTemplateDO>(id);
                }
                unitOfWork.ProcessTemplateRepository.Remove(curProcessTemplate);
                unitOfWork.SaveChanges();
            });
        }
    }
}
