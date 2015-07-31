using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.Exceptions;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers.Api
{
    [Authorize]
    public class ProcessTemplateController : ApiController
    {
	  //IProcessTemplate _processTemplateService;

	  //public ProcessTemplateController()
	  //{
	  //    _processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();
	  //}

	  //// GET api/<controller>
	  //public IEnumerable<ProcessTemplateVM> Get()
	  //{
	  //    IEnumerable<ProcessTemplateDO> ptdoCollection;

	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  ptdoCollection = uow.ProcessTemplateRepository.GetForUser(User.Identity..Name);
	  //	  var ptdos = ptdoCollection.ToList();
	  //	  return ptdos.Select(ptdo => Mapper.Map<ProcessTemplateVM>(ptdo));
	  //    }
	  //}

	  //public IHttpActionResult Get(int Id)
	  //{
	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  var ptdo = uow.ProcessTemplateRepository.GetForUser(Id, User.Identity.Name);
	  //	  if (ptdo != null)
	  //	  {
	  //		return Ok(Mapper.Map<ProcessTemplateVM>(ptdo));
	  //	  }
	  //	  else
	  //	  {
	  //		return NotFound();
	  //	  }
	  //    }
	  //}

	  //public IHttpActionResult Post(ProcessTemplateVM ptvm)
	  //{

	  //    if (string.IsNullOrEmpty(ptvm.Name))
	  //    {
	  //	  ModelState.AddModelError("Name", "Name cannot be null");
	  //    }

	  //    if (!ModelState.IsValid)
	  //    {
	  //	  return BadRequest("Not all data in the request are valid");
	  //    }

	  //    var ptdo = Mapper.Map<ProcessTemplateVM, ProcessTemplateDO>(ptvm);
	  //    ptdo.UserId = User.Identity.Name;

	  //    try
	  //    {
	  //	  _processTemplateService.CreateOrUpdate(ptdo);
	  //	  return Ok();
	  //    }
	  //    catch (EntityNotFoundException)
	  //    {
	  //	  return NotFound();
	  //    }
	  //}

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