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

namespace Web.Controllers
{
    [fr8ApiAuthorize]
    [RoutePrefix("api/manageFile")]
    public class ManageFileController : ApiController
    {
        private readonly IFile _fileService;

        public ManageFileController()
            : this(ObjectFactory.GetInstance<IFile>())
        {
        }

        public ManageFileController(IFile fileService)
        {
            _fileService = fileService;
        }

        public IHttpActionResult Get(int? id = null)
        {
            IList<FileDTO> fileList;

            if (User.IsInRole("Admin"))
            {
                fileList = Mapper.Map<IList<FileDTO>>(_fileService.AllFilesList());
            }
            else
            {
                var userID = User.Identity.GetUserId();
                fileList = Mapper.Map<IList<FileDTO>>(_fileService.FilesList(userID));
            }
            return Ok(fileList);
        }


        public void Delete(int id)
        {
            _fileService.Delete(id);
        }
    }
}