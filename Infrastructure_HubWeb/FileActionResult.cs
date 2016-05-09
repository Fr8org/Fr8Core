using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace HubWeb.Infrastructure
{
    public class FileActionResult : IHttpActionResult
    {
        public FileActionResult(byte[] file)
        {
            this.Content = new MemoryStream(file);
        }
        public FileActionResult(Stream file)
        {
            this.Content = file;
        }

        public Stream Content { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StreamContent(Content);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            return Task.FromResult(response);
        }
    }
}