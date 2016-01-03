using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Services;
using System.Web.Http.Description;

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    //[RoutePrefix("api/fileDetails")]
    public class FileDetailsController : ApiController
    {
        private readonly IFile _fileService;
        private readonly ISecurityServices _security;

        public FileDetailsController()
            : this(ObjectFactory.GetInstance<IFile>())
        {
        }

        public FileDetailsController(IFile fileService)
        {
            _fileService = fileService;
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        [HttpGet]
        //[Route("getDetails/{id:int}")]
        [ActionName("getDetails")]
        [ResponseType(typeof(FileDTO))]
        public IHttpActionResult GetDetails(int id)
        {
            FileDTO fileDto = null;

            if (_security.IsCurrentUserHasRole(Roles.Admin))
            {
                fileDto = Mapper.Map<FileDTO>(_fileService.GetFileByAdmin(id));
            }
            else
            {
                string userId;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    userId = _security.GetCurrentAccount(uow).Id;
                }

                fileDto = Mapper.Map<FileDTO>(_fileService.GetFile(id, userId));
            }

            return Ok(fileDto);
        }
    }
}