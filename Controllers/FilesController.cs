using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Web.Controllers
{
    
    public class FilesController : ApiController
    {
        private readonly IFile _fileService;

        public FilesController(IFile fileService) {
            _fileService = fileService;
        }
    
        [HttpPost]
        [Route("files")]
        public IHttpActionResult Post()
        {
            Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((tsk) =>
            {
                MultipartMemoryStreamProvider prvdr = tsk.Result;

                foreach (HttpContent ctnt in prvdr.Contents)
                {
                    // You would get hold of the inner memory stream here
                    Stream stream = ctnt.ReadAsStreamAsync().Result;
                    var originalFileName = ctnt.Headers.ContentDisposition.FileName.Replace("\"", string.Empty); 
                }
            });

            return Ok();
        }
    }

}
