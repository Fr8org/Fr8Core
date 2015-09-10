using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Web.Controllers
{
    [RoutePrefix("files")]
    public class FileController : ApiController
    {
        private readonly IFile _file;

        public FileController()
        {
            _file = ObjectFactory.GetInstance<IFile>();
        }

        [ResponseType(typeof(FileDescriptionDTO))]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new InvalidDataException("Multipart content data is required to upload files");
            }

            //Read File to Memorystream
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
            {
                throw new InvalidDataException("Expected to receive a single file but received either more or less");
            }

            var file = provider.Contents.First();
            var curFile = new FileDO();
            _file.Store(curFile, await file.ReadAsStreamAsync(), file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty));
            return Ok(Mapper.Map<FileDO, FileDescriptionDTO>(curFile));
        }

        [ResponseType(typeof(List<FileDescriptionDTO>))]
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUploadedFiles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileList = uow.FileRepository.GetAll().Select(Mapper.Map<FileDescriptionDTO>);
                return Ok(fileList);
            }
        }
    }
}
