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

namespace terminalUtilities.Excel
{
    public class ExcelUtils
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly ICrateManager _crateManager;

        public ExcelUtils(IRestfulServiceClient restfulServiceClient, ICrateManager crateManager)
        {
            _restfulServiceClient = restfulServiceClient;
            _crateManager = crateManager;
        }

        public static void ConvertToCsv(string pathToExcel, string pathToCsv)
        {
            if (pathToExcel == null)
                throw new ArgumentNullException("pathToExcel");
            if (pathToExcel == string.Empty)
                throw new ArgumentException("pathToExcel is emtpy", "pathToExcel");
            string ext = Path.GetExtension(pathToExcel);
            if (ext != ".xls" && ext != ".xlsx")
                throw new ArgumentException("Expected '.xls' or '.xlsx'", "pathToExcel");
            if (!File.Exists(pathToExcel))
                throw new FileNotFoundException("File not found", pathToExcel);
            if (pathToCsv == null)
                throw new ArgumentNullException("pathToCsv");
            if (pathToCsv == string.Empty)
                throw new ArgumentException("pathToCsv is emtpy", "pathToCsv");
            if (File.Exists(pathToCsv))
                throw new IOException(string.Format("CSV file '{0}' already exists", pathToCsv));

            using (FileStream stream = new FileStream(pathToExcel, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = null;
                if (ext == ".xls")
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                else
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = true;
                    var dataSet = excelReader.AsDataSet();
                    var table = dataSet.Tables[0];

                    using (FileStream csvStream = File.Open(pathToCsv, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter sw = new StreamWriter(csvStream))
                        {
                            // Write header at first line
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                sb.Append(table.Columns[i]);
                                if (i != table.Columns.Count - 1)
                                    sb.Append(',');
                            }
                            sw.WriteLine(sb.ToString());
                            // Write rows
                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                sb.Clear();
                                for (int j = 0; j < table.Columns.Count; j++)
                                {
                                    sb.Append(table.Rows[i].ItemArray[j]);
                                    if (j != table.Columns.Count - 1)
                                        sb.Append(',');
                                }
                                sw.WriteLine(sb.ToString());
                            }
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }
        }

        public async Task<string[]> GetColumnHeaders(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var file = await RetrieveFile(filePath);
            file.Position = 0;
            return GetColumnHeaders(file.ReadAsBytes(), extension);

        }

        public static string[] GetColumnHeaders(byte[] fileBytes, string extension, string sheetName = null)
        {
            using (var fileStream = new MemoryStream(fileBytes))
            {
                var excelReader = extension == ".xls" ? ExcelReaderFactory.CreateBinaryReader(fileStream) : ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = true;
                    var dataSet = excelReader.AsDataSet();

                    DataTable table;
                    if (string.IsNullOrEmpty(sheetName))
                    {
                        table = dataSet.Tables[0];
                    }
                    else
                    {
                        table = null;

                        for (var i = 0; i < dataSet.Tables.Count; ++i)
                        {
                            if (dataSet.Tables[i].TableName == sheetName)
                            {
                                table = dataSet.Tables[i];
                                break;
                            }
                        }

                        if (table == null)
                        {
                            throw new ApplicationException("Specified Sheet was not found.");
                        }
                    }

                    var columnHeaders = new string[table.Columns.Count];
                    for (int i = 0; i < table.Columns.Count; ++i)
                    {
                        columnHeaders[i] = table.Columns[i].ColumnName;
                    }
                    return columnHeaders;
                }
            }
        }

        public static bool DetectContainsHeader(byte[] fileBytes, string extension, string sheetName = null)
        {
            IExcelDataReader excelReader = null;
            using (var byteStream = new MemoryStream(fileBytes))
            {
                if (extension == ".xls")
                    excelReader = ExcelReaderFactory.CreateBinaryReader(byteStream);
                else
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(byteStream);

                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = false;
                    var dataSet = excelReader.AsDataSet();

                    DataTable table;
                    if (string.IsNullOrEmpty(sheetName))
                    {
                        table = dataSet.Tables[0];
                    }
                    else
                    {
                        table = null;

                        for (var i = 0; i < dataSet.Tables.Count; ++i)
                        {
                            if (dataSet.Tables[i].TableName == sheetName)
                            {
                                table = dataSet.Tables[i];
                                break;
                            }
                        }

                        if (table == null)
                        {
                            throw new ApplicationException("Specified Sheet was not found.");
                        }
                    }

                    if (table.Rows.Count > 0)
                    {
                        foreach (var item in table.Rows[0].ItemArray)
                        {
                            if (item is DBNull || (item is string && string.IsNullOrEmpty((string)item)))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// Fetches rows from the excel byte stream and returns as a Dictionary. 
        /// </summary>
        /// <param name="fileBytes">Byte rray representing Excel data.</param>
        /// <param name="extension">Excel file extension.</param>
        /// <returns>Dictionary<string, List<Tuple<string, string>>> => Dictionary<"Row Number", List<Tuple<"Column Number", "Cell Value">>></returns>
        public static Dictionary<string, List<Tuple<string, string>>> GetTabularData(byte[] fileBytes, string extension, bool isFirstRowAsColumnNames = true, string sheetName = null)
        {
            Dictionary<string, List<Tuple<string, string>>> excelRows = new Dictionary<string, List<Tuple<string, string>>>();
            IExcelDataReader excelReader = null;

            using (var byteStream = new MemoryStream(fileBytes))
            {
                if (extension == ".xls")
                    excelReader = ExcelReaderFactory.CreateBinaryReader(byteStream);
                else
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(byteStream);

                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = isFirstRowAsColumnNames;
                    var dataSet = excelReader.AsDataSet();

                    DataTable table;
                    if (string.IsNullOrEmpty(sheetName))
                    {
                        table = dataSet.Tables[0];
                    }
                    else
                    {
                        table = null;
                        for (var i = 0; i < dataSet.Tables.Count; ++i)
                        {
                            if (dataSet.Tables[i].TableName == sheetName)
                            {
                                table = dataSet.Tables[i];
                                break;
                            }
                        }

                        if (table == null)
                        {
                            throw new ApplicationException("Specified Sheet was not found.");
                        }
                    }

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        List<Tuple<string, string>> rowDataList = new List<Tuple<string, string>>();
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            rowDataList.Add(new Tuple<string, string>((j + 1).ToString(), Convert.ToString(table.Rows[i][j] ?? "")));
                        }
                        excelRows[(i + 1).ToString()] = rowDataList;
                    }
                }
            }
            return excelRows;
        }

        public async Task<byte[]> GetExcelFileAsByteArray(string selectedFilePath)
        {
            var fileAsByteArray = await RetrieveFile(selectedFilePath);
            fileAsByteArray.Position = 0;
            return fileAsByteArray.ReadAsBytes();
        }

        public async Task<StandardTableDataCM> GetExcelFile(string selectedFilePath, bool isFirstRowAsColumnNames = true)
        {
            var fileAsByteArray = await GetExcelFileAsByteArray(selectedFilePath);
            return GetExcelFile(fileAsByteArray, selectedFilePath, isFirstRowAsColumnNames);
        }

        public static List<KeyValuePair<int, string>> GetSpreadsheets(byte[] fileBytes, string extension)
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();

            using (var fileStream = new MemoryStream(fileBytes))
            {
                var excelReader = extension == ".xls" ? ExcelReaderFactory.CreateBinaryReader(fileStream) : ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = true;
                    var dataSet = excelReader.AsDataSet();
                    for (int i = 0; i < dataSet.Tables.Count; i++)
                    {
                        result.Add(new KeyValuePair<int, string>(i, dataSet.Tables[i].TableName));
                    }
                }
            }
            return result;
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }


        public StandardTableDataCM GetExcelFile(byte[] fileAsByteArray, string selectedFilePath, bool isFirstRowAsColumnNames = true, string sheetName = null)
        {
            var ext = Path.GetExtension(selectedFilePath);
            // Read file from repository
            // Fetch column headers in Excel file
            var headersArray = GetColumnHeaders(fileAsByteArray, ext, sheetName);

            // Fetch rows in Excel file
            var rowsDictionary = GetTabularData(fileAsByteArray, ext, isFirstRowAsColumnNames, sheetName);

            Crate curExcelPayloadRowsCrateDTO = null;

            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = CreateTableCellPayloadObjects(rowsDictionary, headersArray, isFirstRowAsColumnNames);
                if (rows != null && rows.Count > 0)
                {
                    curExcelPayloadRowsCrateDTO = Crate.FromContent("Excel Payload Rows", new StandardTableDataCM(isFirstRowAsColumnNames, rows.ToArray()));
                }
            }

            var curStandardTableDataMS = (curExcelPayloadRowsCrateDTO != null) ?
               curExcelPayloadRowsCrateDTO.Get<StandardTableDataCM>()
                : new StandardTableDataCM();

            return curStandardTableDataMS;
        }

        private async Task<Stream> RetrieveFile(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            if (ext != ".xls" && ext != ".xlsx")
            {
                throw new ArgumentException("Expected '.xls' or '.xlsx'", "selectedFile");
            }
            return await _restfulServiceClient.DownloadAsync(new Uri(filePath));
        }

        public static List<TableRowDTO> CreateTableCellPayloadObjects(Dictionary<string, List<Tuple<string, string>>> rowsDictionary, string[] headersArray = null, bool includeHeadersAsFirstRow = false)
        {
            var listOfRows = new List<TableRowDTO>();
            if (includeHeadersAsFirstRow)
            {
                listOfRows.Add(new TableRowDTO { Row = headersArray.Select(x => new TableCellDTO { Cell = new KeyValueDTO(x, x) }).ToList() });
            }
            // Process each item in the dictionary and add it as an item in List<TableRowDTO>
            foreach (var row in rowsDictionary)
            {
                var listOfCells = row.Value.Select(x => new TableCellDTO
                {
                    Cell = new KeyValueDTO
                    {
                        Key = headersArray != null ? headersArray[int.Parse(x.Item1) - 1] : x.Item1, // Column header
                        Value = x.Item2 // Column/cell value
                    }
                }).ToList();
                listOfRows.Add(new TableRowDTO { Row = listOfCells });
            }
            return listOfRows;
        }
        
        private static DataTable ToDataTable(StandardTableDataCM tableCM)
        {
            if (tableCM == null || tableCM.Table == null || tableCM.Table.Count == 0)
            {
                throw new ApplicationException("Invalid StandardTableDataCM data.");
            }

            var dataTable = new DataTable();
            var columnIndex = new Dictionary<string, int>();

            for (var i = 0; i < tableCM.Table[0].Row.Count; ++i)
            {
                var cell = tableCM.Table[0].Row[i].Cell;
                dataTable.Columns.Add(cell.Key, typeof(string));

                columnIndex.Add(cell.Key, i);
            }

            foreach (var row in tableCM.Table.Skip(1))
            {
                var dataRow = new object[tableCM.Table[0].Row.Count];

                foreach (var cell in row.Row)
                {
                    int columnNumber;
                    if (columnIndex.TryGetValue(cell.Cell.Key, out columnNumber))
                    {
                        dataRow[columnNumber] = cell.Cell.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public static byte[] CreateExcelFile(StandardTableDataCM tableCM, string sheetName)
        {
            var dataTable = ToDataTable(tableCM);
            dataTable.TableName = sheetName;

            var writer = new ExcelWriter();
            using (var stream = new MemoryStream())
            {
                writer.WriteFile(stream, dataTable);
                return stream.ToArray();
            }
        }

        public static byte[] RewriteSheetForFile(byte[] existingFile,
            StandardTableDataCM tableCM,
            string sheetName)
        {

            using (var memoryStream = new MemoryStream(existingFile))
            using (var excelPackage = new ExcelPackage(memoryStream))
            { 
                var workSheet = excelPackage.Workbook.Worksheets[sheetName];
                if (workSheet == null)
                {
                    workSheet = excelPackage.Workbook.Worksheets.Add(sheetName);
                }
                else
                {
                    var targetIndex = workSheet.Index;
                    excelPackage.Workbook.Worksheets.Delete(workSheet);
                    workSheet = excelPackage.Workbook.Worksheets.Add(sheetName);
                    excelPackage.Workbook.Worksheets.MoveBefore(workSheet.Index, targetIndex);
                }
                WriteTableDataToWorksheet(workSheet, tableCM);
                FixPackageStyles(excelPackage);
                return excelPackage.GetAsByteArray();
            }


            //var dataTable = ToDataTable(tableCM);
            //dataTable.TableName = sheetName;

            //var writer = new ExcelWriter();

            //using (var stream = new MemoryStream())
            //{
            //    stream.Write(existingFile, 0, existingFile.Length);
            //    stream.Position = 0;

            //    writer.RewriteSheetForFile(stream, dataTable);
            //    return stream.ToArray();
            //}
        }

        private static void FixPackageStyles(ExcelPackage excelPackage)
        {
            //This is for cases where Styles property is corrupted
            try
            {
                var _ = excelPackage.Workbook.Styles;
            }
            catch (NullReferenceException)
            {
                using (var fakePackage = new ExcelPackage())
                {
                    excelPackage.Workbook.StylesXml = fakePackage.Workbook.StylesXml;
                }
            }
        }

        private static void WriteTableDataToWorksheet(ExcelWorksheet workSheet, StandardTableDataCM tableCm)
        {
            var table = tableCm.Table;
            for (var row = 0; row < table.Count; row++)
            {
                var currentRow = table[row].Row;
                for (var col = 0; col < currentRow.Count; col++)
                {
                    //CellRange enumerate rows and columns from index 1
                    workSheet.Cells[row + 1, col + 1].Value = currentRow[col].Cell.Value;
                }
            }
        }
    }
}
