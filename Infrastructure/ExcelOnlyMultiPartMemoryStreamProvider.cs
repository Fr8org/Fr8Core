using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Web.Infrastructure
{
    public class ExcelOnlyMultiPartMemoryStreamProvider : MultipartMemoryStreamProvider
    {
        private static readonly List<String> _allowedExtensions = new List<string>{".xls, .xlsx"}; 
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            var fileExtension = Path.GetExtension(headers.ContentDisposition.FileName);
            var isExcelFile = _allowedExtensions == null || _allowedExtensions.Any(i => i.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));
            if (!isExcelFile)
            {
                throw new InvalidDataException("It is only allowed to upload excel (xls, xlsx) files");
            }

            return base.GetStream(parent, headers);
        }
    }
}