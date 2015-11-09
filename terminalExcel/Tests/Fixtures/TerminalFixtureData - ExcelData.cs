using System;
using System.Collections.Generic;
using System.IO;
using DocuSign.Integrations.Client;
using System.IO;
using Utilities;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using terminalExcel.Infrastructure;
using StructureMap;
using Data.Interfaces;

using Data.Repositories;
using Hub.Interfaces;
using Utilities;
using terminalExcel.Infrastructure;

namespace terminalExcel.Fixtures
{
    public partial class TerminalFixtureData
    {
        public static byte[] TestExcelData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IFileRepository fileRepository = new FileRepository(uow);
                var blobUrl = "https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx";
                try
                {
                    var byteArray = fileRepository.GetRemoteFile(blobUrl);
                    return byteArray;
                }
                finally
                {
                }
            }
        }

        public static string[] TestColumnHeaders()
        {
            string pathToExcel = @"..\..\Fixtures\Sample Files\SampleFile1.xlsx";
            var byteArray = File.ReadAllBytes(pathToExcel);
            return ExcelUtils.GetColumnHeaders(byteArray, "xlsx");
        }

        public static Dictionary<string, List<Tuple<string, string>>> TestRows()
        {
            string pathToExcel = @"..\..\Fixtures\Sample Files\SampleFile1.xlsx";
            var byteArray = File.ReadAllBytes(pathToExcel);
            return ExcelUtils.GetTabularData(byteArray, "xlsx");
        }
    }
}
