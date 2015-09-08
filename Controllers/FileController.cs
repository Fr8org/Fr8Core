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
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Web.Infrastructure;

namespace Web.Controllers
{
    [RoutePrefix("api/files")]
    public class FileController : ApiController
    {

        [ResponseType(typeof(FileDTO))]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadExcelFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                //TODO maybe we should create an event handler for this and log these messages
                return BadRequest("Multipart content data is required to upload files");
            }

            var provider = new ExcelOnlyMultiPartMemoryStreamProvider();
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            //this exception is thrown by ExcelOnlyMultiPartMemoryStreamProvider when file extension is not an excel extension
            catch (InvalidDataException exc) 
            {
                return BadRequest(exc.Message);
            }

            if (provider.Contents.Count != 1)
            {
                return BadRequest("It is only allowed to upload single excel file");
            }

            try
            {
                var excelFile = provider.Contents.First();
                var fileService = ObjectFactory.GetInstance<IFile>();
                var curFile = new FileDO();
                //upload file to azure and save to db
                fileService.Store(curFile, await excelFile.ReadAsStreamAsync(), excelFile.Headers.ContentDisposition.FileName.Replace("\"", string.Empty));
                return Ok(Mapper.Map<FileDO, FileDTO>(curFile));
            }
            catch (Exception e)
            {
                //perhaps we couldn't upload file to azure
                //TODO log exception
                return InternalServerError(e);

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
