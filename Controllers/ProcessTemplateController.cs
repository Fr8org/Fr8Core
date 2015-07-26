using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Web.ViewModels;
using AutoMapper;
using StructureMap;
using Data.Interfaces;
using Data.States;
using Core.Managers;
using Core.Exceptions;

namespace Web.Controllers
{
    [Authorize]
    public class ProcessTemplateController : Controller
    {
        string generalErrorMessage = "Unfortunately, we're unable to save your changes at this moment. Please try again in a few minutes.";

        // GET: ProcessTemplate
        public ActionResult Index()
        {
            IEnumerable<ProcessTemplateDO> ptdoCollection;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ptdoCollection = uow.ProcessTemplateRepository.GetForUser(User.Identity.Name);
            }

            if (ptdoCollection == null)
            {
                ModelState.AddModelError("", "Process Template with the specified ID is not found. Please make sure that it exists.");
                return RedirectToAction("Index");
            }
            
            return View(ptdoCollection.Select(ptdo => Mapper.Map<ProcessTemplateVM>(ptdo)));
        }

        // GET: ProcessTemplate/Details/5
        public ActionResult Details(int Id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetForUser(Id, User.Identity.Name);
                return View(Mapper.Map<ProcessTemplateVM>(ptdo));
            }
        }

        // GET: ProcessTemplate/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProcessTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProcessTemplateVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                CreateOrUpdate(vm);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", generalErrorMessage);
                return View();
            }
        }

        // POST: ProcessTemplate/Edit/5
        [HttpPost]
        public ActionResult Edit(ProcessTemplateVM vm)
        {
            AlertReporter alertReporter = ObjectFactory.GetInstance<AlertReporter>();
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                CreateOrUpdate(vm);
                return RedirectToAction("Index");
            }
            catch (EntityNotFoundException)
            {
                ModelState.AddModelError("", "We're unable to save your changes. Please make sure that Process Template exists.");
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", generalErrorMessage);
                return View();
            }
        }

        // GET: ProcessTemplate/Edit/5
        public ActionResult Edit(int id)
        {
            ProcessTemplateDO ptdo;
            if (id == 0)
            {
                ModelState.AddModelError("", "Please specify ID of the record to edit.");
                return RedirectToAction("Index");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ptdo = uow.ProcessTemplateRepository.GetForUser(id, User.Identity.Name);
            }

            if (ptdo == null)
            {
                ModelState.AddModelError("", "Process Template with the specified ID is not found. Please make sure that it exists.");
                return RedirectToAction("Index");
            }

            var ptvm = Mapper.Map<ProcessTemplateVM>(ptdo);

            return View(ptvm);
        }

        // GET: ProcessTemplate/Delete/5
        public ActionResult Delete(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = uow.ProcessTemplateRepository.GetForUser(id, User.Identity.Name);
                if (ptdo == null)
                {
                    ModelState.AddModelError("", "Process Template with the specified ID is not found. Please make sure that it exists.");
                    return RedirectToAction("Index");
                }
                uow.ProcessTemplateRepository.Remove(ptdo);
                uow.SaveChanges();
            }

            return View();
        }

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
