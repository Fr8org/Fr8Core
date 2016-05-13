using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure;
using Fr8Data.DataTransferObjects;
using HubWeb.Infrastructure_HubWeb;

namespace HubWeb.Controllers
{
    //
    public class FilesController : ApiController
    {
        private readonly IFile _fileService;
        private readonly ISecurityServices _security;
        private readonly ITag _tagService;

        public FilesController() : this(ObjectFactory.GetInstance<IFile>()) { }

        public FilesController(IFile fileService)
        {
            _fileService = fileService;
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _tagService = ObjectFactory.GetInstance<ITag>();
        }

        [HttpPost]
        [ActionName("files")]
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public async Task<IHttpActionResult> Post()
        {
            FileDO fileDO = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUserId = _security.GetCurrentUser();

                await Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((tsk) =>
                {
                    MultipartMemoryStreamProvider prvdr = tsk.Result;

                    foreach (HttpContent ctnt in prvdr.Contents)
                    {
                        Stream stream = ctnt.ReadAsStreamAsync().Result;
                        var fileName = ctnt.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                        fileDO = new FileDO
                        {
                            DockyardAccountID = currentUserId
                        };

                        _fileService.Store(uow, fileDO, stream, fileName);

                    }
                });

                return Ok(fileDO);
            }
        }

        [HttpGet]
        //[Route("files/details/{id:int}")]
        [ActionName("details")]
        [ResponseType(typeof(FileDTO))]
        public IHttpActionResult Details(int id)
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

        /// <summary>
        /// Downloads user's given file
        /// </summary>
        /// <param name="id">id of requested file</param>
        /// <returns>Filestream</returns>
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        [HttpGet]
        public IHttpActionResult Download(int id)
        {
            FileDO fileDO = null;
            if (_security.IsCurrentUserHasRole(Roles.Admin))
            {
                fileDO = _fileService.GetFileByAdmin(id);
            }
            else
            {
                string userId;
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    userId = _security.GetCurrentAccount(uow).Id;
                }
                fileDO = _fileService.GetFile(id, userId);
            }
            if (fileDO == null)
            {
                return NotFound();
            }
            var file = _fileService.Retrieve(fileDO);
            return new FileActionResult(file);
        }

        /// <summary>
        /// Gets all files current user stored on Fr8
        /// </summary>
        /// <returns>List of FileDTO</returns>
        [Fr8HubWebHMACAuthenticate]
        [Fr8ApiAuthorize]
        public IHttpActionResult Get()
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

            // fills Tags property for each fileDTO to display in the Tags column
            // example: { "key1" : "value1" }, {"key2", "value2} ...
            foreach (var file in fileList)
            {
                var result = String.Empty;
                var tags = _tagService.GetList(file.Id);
                bool isFirstItem = true;
                foreach (var tag in tags)
                {
                    if (isFirstItem)
                    {
                        isFirstItem = false;
                    }
                    else
                    {
                        result += ", ";
                    }
                    result += "{\"" + tag.Key + "\" : \"" + tag.Value + "\"}";
                }
                file.Tags = result;
            }

            return Ok(fileList);
        }

        public void Delete(int id)
        {
            _fileService.Delete(id);
        }
    }
}
