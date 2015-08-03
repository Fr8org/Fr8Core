using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Ajax.Utilities;
using StructureMap;
using Web.ViewModels;

namespace Web.Controllers
{
    [Authorize]
    public class ProcessTemplateController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get(int? id = null)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<ProcessTemplateDO> curProcessTemplates =
                    unitOfWork.ProcessTemplateRepository.GetForUser(User.Identity.Name, id);

                var ptdos = curProcessTemplates.ToList();
                switch (ptdos.Count)
                {
                    case 0:
                        throw new ApplicationException("Process Template not found for id {0}".FormatInvariant(id));
                    case 1:
                        return Ok(Mapper.Map<ProcessTemplateDTO>(ptdos.First()));
                }

                return Ok(ptdos.Select(Mapper.Map<ProcessTemplateDTO>));
            }
        }



        public IHttpActionResult Post(ProcessTemplateDTO ptvm)
        {

            if (string.IsNullOrEmpty(ptvm.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Some of the request data is invalid");
            }

            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var ptdo = Mapper.Map<ProcessTemplateDTO, ProcessTemplateDO>(ptvm);
                ptdo.UserId = User.Identity.Name;
                ptvm.Id = unitOfWork.ProcessTemplateRepository.CreateOrUpdate(ptdo);
                return Ok(ptvm);
            }

           
            

        }

        public IHttpActionResult Delete(int id)
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                unitOfWork.ProcessTemplateRepository.Delete(id);
                return Ok();
            }
        }

        
    }
}