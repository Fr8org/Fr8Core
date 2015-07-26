using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.Exceptions;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.Api
{
    public class ProcessTemplateController : ApiController
    {
        EventReporter _eventReporter = new EventReporter();

        public ProcessTemplateController()
        {
            _eventReporter = ObjectFactory.GetInstance<EventReporter>();
        }

        // GET api/<controller>
        public IEnumerable<ProcessTemplateVM> Get()
        {
            IEnumerable<ProcessTemplateDO> ptdoCollection;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ptdoCollection = uow.ProcessTemplateRepository.GetForUser(User.Identity.Name);

                var ptdos = ptdoCollection.ToList();

                return ptdos.Select(ptdo => Mapper.Map<ProcessTemplateVM>(ptdo));
            }
        }

        public IHttpActionResult Get(int Id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetForUser(Id, User.Identity.Name);
                if (ptdo != null)
                {
                    return Ok(Mapper.Map<ProcessTemplateVM>(ptdo));
                }
                else
                {
                    return NotFound();
                }
            }
        }

        public IHttpActionResult Post(ProcessTemplateVM ptvm)
        {
            bool creating = ptvm.Id == 0;

            if (string.IsNullOrEmpty(ptvm.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Not all data in the request are valid");
            }

            try
            {
                CreateOrUpdate(ptvm);
                if (creating)
                {
                    //TODO: Uncomment when error with EventReporter is fixed.
                    //_eventReporter.ProcessTemplateCreated(User.Identity.Name, ptvm.Name);
                }
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        public IHttpActionResult Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetForUser(id, User.Identity.Name);
                if (ptdo == null)
                {
                    return NotFound();
                }
                uow.ProcessTemplateRepository.Remove(ptdo);
                uow.SaveChanges();
            }

            return Ok();
        }

        /// <summary>
        /// The procedure either creates or updates an existing Process Template
        /// depending on there Id is present on ProcessTemplateVM object.
        /// </summary>
        private void CreateOrUpdate(ProcessTemplateVM viewModel)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Mapper.CreateMap<ProcessTemplateVM, ProcessTemplateDO>()
                   .ConstructUsing((ProcessTemplateVM vm) =>
                   {
                       ProcessTemplateDO entity;
                       if (vm.Id == 0)
                       {
                           entity = new ProcessTemplateDO();
                           entity.UserId = User.Identity.Name;
                           entity.ProcessState = ProcessTemplateState.Active;
                           uow.ProcessTemplateRepository.Add(entity);
                           return entity;
                       }
                       entity = uow.ProcessTemplateRepository.GetForUser(vm.Id, User.Identity.Name);

                       if (entity == null)
                       {
                           throw new EntityNotFoundException();
                       }
                       else
                       {
                           return entity;
                       }
                   });
                Mapper.Map<ProcessTemplateVM, ProcessTemplateDO>(viewModel);
                uow.SaveChanges();
            }
        }
    }
}