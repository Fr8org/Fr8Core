using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using OfficeOpenXml;
using StructureMap;
using RestSharp.Extensions;

namespace terminalUtilities.Files
{
    public class FileUtils
    {
        private readonly IRestfulServiceClient _restfulServiceClient;

        public FileUtils(IRestfulServiceClient restfulServiceClient, ICrateManager crateManager)
        {
            _restfulServiceClient = restfulServiceClient;
        }

       
        public async Task<byte[]> GetFileAsByteArray(string selectedFilePath, params string[] extensions)
        {
            var fileAsByteArray = await RetrieveFile(selectedFilePath, extensions);
            fileAsByteArray.Position = 0;
            return fileAsByteArray.ReadAsBytes();
        }
        public async Task<System.IO.Stream> RetrieveFile(string filePath, params string[] extensions)
        {
            var ext = System.IO.Path.GetExtension(filePath);
            string exception = "Expected ";
            bool hasException = false;
            foreach (var extension in extensions)
            {
                if(ext == extension)
                {
                    hasException = false;
                    break;
                }
                if (ext != extension)
                {
                    hasException = true;
                    if (extensions.First() == extension)
                    {
                        exception += extension;
                    }
                    else
                    {
                        exception += " or " + extension;
                    }
                }
            }
            if (hasException == true)
            {
                throw new ArgumentException(exception, "selectedFile");
            }

            return await _restfulServiceClient.DownloadAsync(new Uri(filePath));
        }
    }
}

