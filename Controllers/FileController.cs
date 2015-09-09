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
    [RoutePrefix("api/files")]
    public class FileController : ApiController
    {

        [ResponseType(typeof(FileDTO))]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                //TODO maybe we should create an event handler for this and log these messages
                throw new InvalidDataException("Multipart content data is required to upload files");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.Contents.Count != 1)
            {
                throw new InvalidDataException("It is only allowed to upload single file");
            }

            try
            {
                var _file = provider.Contents.First();
                var fileService = ObjectFactory.GetInstance<IFile>();
                var curFile = new FileDO();
                //upload file to azure and save to db
                fileService.Store(curFile, await _file.ReadAsStreamAsync(), _file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty));
                return Ok(Mapper.Map<FileDO, FileDTO>(curFile));
            }
            catch (Exception e)
            {
                //perhaps we couldn't upload file to azure
                //TODO log exception
                throw new Exception("File upload was failed. Please retry.");

            }
        }

        [ResponseType(typeof(List<FileDTO>))]
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUploadedFiles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fileList = uow.FileRepository.GetAll().Select(Mapper.Map<FileDTO>);
                return Ok(fileList);
            }
        }
    }
}
