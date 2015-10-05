using System;
using DocuSign.Integrations.Client;
using System.IO;
using Utilities;
using Data.Interfaces.ManifestSchemas;
using Core.Interfaces;
using System.Collections.Generic;

namespace pluginTests.Fixtures
{
	public partial class PluginFixtureData
	{
        public static byte[] TestExcelData()
		{
            string pathToExcel = @"..\..\Fixtures\Sample Files\SampleFile1.xlsx";
            var byteArray = File.ReadAllBytes(pathToExcel);
            return byteArray;
		}

        public static string[] TestColumnHeaders()
        {
            string pathToExcel = @"..\..\Fixtures\Sample Files\SampleFile1.xlsx";
            var byteArray = File.ReadAllBytes(pathToExcel);
            return ExcelUtils.GetColumnHeaders(byteArray, "xlsx"); ;
        }

        public static Dictionary<string, List<Tuple<string, string>>> TestRows()
        {
            string pathToExcel = @"..\..\Fixtures\Sample Files\SampleFile1.xlsx";
            var byteArray = File.ReadAllBytes(pathToExcel);
            return ExcelUtils.GetTabularData(byteArray, "xlsx"); ;
        }
	}
}
