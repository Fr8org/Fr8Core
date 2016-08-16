using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
using Fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    
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
        /// <summary>
        /// Uploads the file content to Azure Blob storage and then saves the file object to the Fr8 database
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Returns the description of file that was succesfully uploaded</response>
        /// <response code="403">Unauthorized request</response>
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(FileDO))]
        public async Task<IHttpActionResult> Post()
        {
            FileDO fileDO = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUserId = _security.GetCurrentUser();

                await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider()).ContinueWith(tsk =>
                {
                    var prvdr = tsk.Result;

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
        /// <summary>
        /// Retrieves file description by specified Id of file and owned by current user
        /// </summary>
        /// <param name="id">Id of file to retrieve description for</param>
        /// <reponse code="200">Return the description of file</reponse>
        [HttpGet]
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
        /// Downloads file with specified Id and owned by current user
        /// </summary>
        /// <param name="id">Id of requested file</param>
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.OK, "Contents of specified file as byte array")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unathorized request")]
        [SwaggerResponse(HttpStatusCode.NotFound, "File with specified Id doesn't exist")]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult Get(int id)
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
        /// Downloads file with specified path and owned by current user
        /// </summary>
        /// <param name="path">Path of the requested file</param>
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [ActionName("byPath")]
        [SwaggerResponse(HttpStatusCode.OK, "Contents of specified file as byte array")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unathorized request")]
        [SwaggerResponse(HttpStatusCode.NotFound, "File at specified path doesn't exist")]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult DownloadFileByPath(string path)
        {
            FileDO fileDO = null;
            if (_security.IsCurrentUserHasRole(Roles.Admin))
            {
                fileDO = new FileDO { CloudStorageUrl = path };
            }
            else
            {
                string userId;
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    userId = _security.GetCurrentAccount(uow).Id;
                }
                fileDO = _fileService.GetFile(path, userId);
            }
            if (fileDO == null)
            {
                return NotFound();
            }
            var file = _fileService.Retrieve(fileDO);
            return new FileActionResult(file);
        }

        /// <summary>
        /// Retrieves the list of file descriptions owned by current user
        /// </summary>
        /// <response code="200">Collection of files</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(IList<FileDTO>))]
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
        /// <summary>
        /// Deletes file with specified Id
        /// </summary>
        /// <param name="id">Id of the file specified</param>
        /// <response code="204">File was succesfully deleted</response>
        [Fr8TerminalAuthentication]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.NoContent, "File was successfully delete")]
        [SwaggerResponseRemoveDefaults]
        public void Delete(int id)
        {
            _fileService.Delete(id);
        }
    }
}
