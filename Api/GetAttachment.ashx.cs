using System;
using System.Linq;
using System.Web;
using System.Web.Services;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace KwasantWeb.Api
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class GetAttachment : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            int attachmentID = Convert.ToInt32(context.Request.Params["AttachmentID"]);

            HttpRequestBase request = new HttpRequestWrapper(context.Request);
            HttpResponseBase response = new HttpResponseWrapper(context.Response);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var attachment = uow.AttachmentRepository.GetQuery().FirstOrDefault(e => e.Id == attachmentID);

            CreateFileResponse(response, attachment);
        }

        private void CreateFileResponse(HttpResponseBase response, AttachmentDO attachment)
        {
            var fileData = attachment.Bytes;
            
            response.ContentType = "application/octet-stream";
            response.AddHeader("Content-Disposition", "filename=\"" + attachment.OriginalName + "\"");
            response.AddHeader("Content-Length", fileData.LongLength.ToString());
            response.AddHeader("Content-Type", attachment.Type + "; name=\"" + attachment.OriginalName + "\";");
            //Write the data
            response.BinaryWrite(fileData);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
