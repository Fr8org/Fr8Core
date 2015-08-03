using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.Exceptions;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    [Authorize]
    public class ProcessTemplateController : ApiController
    {
        readonly IProcessTemplate _processTemplateService;

        public ProcessTemplateController()
        {
            _processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();
        }

        // GET api/<controller>
        public IHttpActionResult  Get(int? id = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<ProcessTemplateDO> ptdoCollection = 
                    uow.ProcessTemplateRepository.GetForUser(User.Identity.Name, id);

                var ptdos = ptdoCollection.ToList();
                switch (ptdos.Count)
                {
                    case 0:
                        return NotFound();
                    case 1:
                        return Ok(Mapper.Map<ProcessTemplateVM>(ptdos.First()));
                }

                return Ok(ptdos.Select(Mapper.Map<ProcessTemplateVM>));
            }
        }



        public IHttpActionResult Post(ProcessTemplateVM ptvm)
        {

            if (string.IsNullOrEmpty(ptvm.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Not all data in the request are valid");
            }

            var ptdo = Mapper.Map<ProcessTemplateVM, ProcessTemplateDO>(ptvm);
            
            ptdo.UserId = User.Identity.Name;

            try
            {
                _processTemplateService.CreateOrUpdate(ptdo);
                ptvm.Id = ptdo.Id;
                return Ok(ptvm);
            }
            catch (Exception exception)
            {
                 var error = new HttpError(exception,true);
                 return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, error));
             }
        }

        //public IHttpActionResult Delete(int id)
        //{
        //    try
        //    {
        //	  _processTemplateService.Delete(id, User.Identity.Name);
        //	  return Ok();
        //    }
        //    catch (EntityNotFoundException)
        //    {
        //	  return NotFound();
        //    }
        //}

        //private void CreateOrUpdate(ProcessTemplateVM viewModel)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        Mapper.CreateMap<ProcessTemplateVM, ProcessTemplateDO>()
        //           .ConstructUsing((ProcessTemplateVM vm) =>
        //           {
        //               ProcessTemplateDO entity;
        //               if (vm.Id == 0)
        //               {
        //                   entity = new ProcessTemplateDO();
        //                   entity.UserId = User.Identity.Name;
        //                   entity.ProcessState = ProcessTemplateState.Active;
        //                   uow.ProcessTemplateRepository.Add(entity);
        //                   return entity;
        //               }
        //               entity = uow.ProcessTemplateRepository.GetForUser(vm.Id, User.Identity.Name);

        //               if (entity == null)
        //               {
        //                   throw new EntityNotFoundException();
        //               }
        //               else
        //               {
        //                   return entity;
        //               }
        //           });
        //        Mapper.Map<ProcessTemplateVM, ProcessTemplateDO>(viewModel);
        //        uow.SaveChanges();
        //    }
        //}
    }
}