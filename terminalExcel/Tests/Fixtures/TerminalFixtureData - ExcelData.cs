using System;
using System.Collections.Generic;
using System.IO;
using StructureMap;
using terminalUtilities.Excel;

namespace terminalExcel.Fixtures
{
    public partial class TerminalFixtureData
    {
       /* public static byte[] TestExcelData()
        {
            var cloudFileManager = ObjectFactory.GetInstance<CloudFileManager>();
            var blobUrl = "https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx";
            try
            {
                var byteArray = cloudFileManager.GetRemoteFile(blobUrl);
                return byteArray;
            }
            finally
            {
            }
        }*/

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
