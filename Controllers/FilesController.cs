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

namespace Web.Controllers
{
    [fr8ApiAuthorize]
    public class FilesController : ApiController
    {
        private readonly IFile _fileService;

        public FilesController() : this(ObjectFactory.GetInstance<IFile>()) { }

        public FilesController(IFile fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        [Route("files")]
        public async Task<IHttpActionResult> Post()
        {
            FileDO fileDO = null;
            await Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((tsk) =>
            {
                MultipartMemoryStreamProvider prvdr = tsk.Result;

                foreach (HttpContent ctnt in prvdr.Contents)
                {
                    Stream stream = ctnt.ReadAsStreamAsync().Result;
                    var fileName = ctnt.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                    fileDO = new FileDO
                    {
                        DockyardAccountID = User.Identity.GetUserId()
                    };
                    _fileService.Store(fileDO, stream, fileName);
                    
                }
            });
            
            return Ok(fileDO);
            
        }
    }
}
