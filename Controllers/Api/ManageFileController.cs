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

namespace HubWeb.Controllers
{
    [Fr8ApiAuthorize]
    //[RoutePrefix("api/manageFile")]
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

            int i = 1;
            foreach (var file in fileList)
            {
                file.Tags = "tag" + i.ToString();
                i++;
            }

            return Ok(fileList);
        }


        public void Delete(int id)
        {
            _fileService.Delete(id);
        }
    }
}