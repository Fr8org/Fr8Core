using Core.Interfaces;
using Core.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Data.Interfaces.DataTransferObjects;
using AutoMapper;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;

namespace Web.Controllers
{
    [Fr8ApiAuthorize]
    [RoutePrefix("api/manageFile")]
    public class ManageFileController : ApiController
    {
        private readonly IFile _fileService;
        private readonly ISecurityServices _security;

        public ManageFileController()
            : this(ObjectFactory.GetInstance<IFile>())
        {
        }

        public ManageFileController(IFile fileService)
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _fileService = fileService;
        }

        public IHttpActionResult Get(int? id = null)
        {
            IList<FileDTO> fileList;

            if (_security.IsCurrentUserHasRole(Roles.Admin))
            {
                fileList = Mapper.Map<IList<FileDTO>>(_fileService.AllFilesList());
            }
            else
            {
                string userId;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    userId = _security.GetCurrentAccount(uow).Id;
                }

                fileList = Mapper.Map<IList<FileDTO>>(_fileService.FilesList(userId));
            }

            return Ok(fileList);
        }


        public void Delete(int id)
        {
            _fileService.Delete(id);
        }
    }
}