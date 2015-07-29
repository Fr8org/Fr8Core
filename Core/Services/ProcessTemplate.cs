using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    public class ProcessTemplate : IProcessTemplate
    {
        EventReporter _eventReporter;

        public ProcessTemplate()
        {
            _eventReporter = ObjectFactory.GetInstance<EventReporter>();
        }

        public void Delete(int id, string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetForUser(id, userId);
                if (ptdo == null)
                {
                    throw new EntityNotFoundException();
                }
                uow.ProcessTemplateRepository.Remove(ptdo);
                uow.SaveChanges();
            }

        }

        public void CreateOrUpdate(ProcessTemplateDO ptdo)
        {
            if (string.IsNullOrEmpty(ptdo.UserId))
            {
                throw new ArgumentNullException("ptdo.UserId");
            }

            bool creating = ptdo.Id == 0;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (ptdo.Id == 0)
                {
                    uow.ProcessTemplateRepository.Add(ptdo);
                }
                else
                {
                    var entity = uow.ProcessTemplateRepository.GetForUser(ptdo.Id, ptdo.UserId);

                    if (entity == null)
                    {
                        throw new EntityNotFoundException();
                    }
                    else
                    {
                        entity.Name = ptdo.Name;
                        entity.Description = ptdo.Description;
                    }
                }
                uow.SaveChanges();
            }

            if (creating)
            {
                _eventReporter.ProcessTemplateCreated(ptdo.UserId, ptdo.Name);
            }
        }
    }
}
