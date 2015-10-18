using Core.Interfaces;
using Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using StructureMap;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;

namespace Web.Controllers
{
    [Fr8ApiAuthorize]
    public class FilesController : ApiController
    {
        private readonly IFile _fileService;
        private readonly ISecurityServices _security;

        public FilesController() : this(ObjectFactory.GetInstance<IFile>()) { }

        public FilesController(IFile fileService)
        {
            _fileService = fileService;
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        [HttpPost]
        [Route("files")]
        public async Task<IHttpActionResult> Post()
        {
            FileDO fileDO = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var account = _security.GetCurrentAccount(uow);

                await Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((tsk) =>
                {
                    MultipartMemoryStreamProvider prvdr = tsk.Result;

                    foreach (HttpContent ctnt in prvdr.Contents)
                    {
                        Stream stream = ctnt.ReadAsStreamAsync().Result;
                        var fileName = ctnt.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                        fileDO = new FileDO
                        {
                            DockyardAccountID = account.Id
                        };

                        _fileService.Store(uow, fileDO, stream, fileName);

                    }
                });

                return Ok(fileDO);
            }
        }
    }
}
