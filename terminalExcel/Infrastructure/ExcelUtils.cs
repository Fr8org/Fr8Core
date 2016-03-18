using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Excel;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers;

namespace terminalExcel.Infrastructure
{
	public static class ExcelUtils
	{
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

        public static string[] GetColumnHeaders(byte[] fileBytes, string extension)
        {
            IExcelDataReader excelReader = null;
            string[] columnHeaders;
             
            using (var fileStream = new MemoryStream(fileBytes))
            {
                if (extension == ".xls")
                    excelReader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                else
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                using (excelReader)
                {
                    excelReader.IsFirstRowAsColumnNames = true;
                    var dataSet = excelReader.AsDataSet();
                    var table = dataSet.Tables[0];
                    columnHeaders = new string[table.Columns.Count];

                    for (int i = 0; i < table.Columns.Count; ++i)
                    {
                        columnHeaders[i] = table.Columns[i].ColumnName;
                    }
                }
            }
            return columnHeaders;
        }

        /// <summary>
        /// Fetches rows from the excel byte stream and returns as a Dictionary. 
        /// </summary>
        /// <param name="fileBytes">Byte rray representing Excel data.</param>
        /// <param name="extension">Excel file extension.</param>
        /// <returns>Dictionary<string, List<Tuple<string, string>>> => Dictionary<"Row Number", List<Tuple<"Column Number", "Cell Value">>></returns>
        public static Dictionary<string, List<Tuple<string, string>>> GetTabularData(byte[] fileBytes, string extension, bool isFirstRowAsColumnNames = true)
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
                    var table = dataSet.Tables[0];

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

        public static StandardTableDataCM GetTableData(string selectedFilePath)
        {
            // Check if the file is an Excel file.
            string ext = Path.GetExtension(selectedFilePath);
            if (ext != ".xls" && ext != ".xlsx")
                throw new ArgumentException("Expected '.xls' or '.xlsx'", "selectedFile");

            FileDO curFileDO = new FileDO()
            {
                CloudStorageUrl = selectedFilePath,
            };

            IFile file = ObjectFactory.GetInstance<IFile>();
            ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();

            // Read file from repository
            var fileAsByteArray = file.Retrieve(curFileDO);

            // Fetch column headers in Excel file
            var headersArray = ExcelUtils.GetColumnHeaders(fileAsByteArray, ext);

            // Fetch rows in Excel file
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, ext);

            Crate curExcelPayloadRowsCrateDTO = null;
            
            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = CreateTableCellPayloadObjects(rowsDictionary, headersArray);
                if (rows != null && rows.Count > 0)
                {
                    curExcelPayloadRowsCrateDTO = crate.CreateStandardTableDataCrate("Excel Payload Rows", true, rows.ToArray());
                }
            }

            var curStandardTableDataMS = (curExcelPayloadRowsCrateDTO != null) ?
               curExcelPayloadRowsCrateDTO.Get<StandardTableDataCM>()
                : new StandardTableDataCM();

            return curStandardTableDataMS;
        }

        public static List<TableRowDTO> CreateTableCellPayloadObjects(Dictionary<string, List<Tuple<string, string>>> rowsDictionary, string[] headersArray = null)
        {
            try
            {
                var listOfRows = new List<TableRowDTO>();
                // Add header as the first row in List<TableRowDTO> so that it is becomes the first row in StandardTableMS.
                if(headersArray != null)
                { 
                    var headerTableRowDTO = new TableRowDTO() { Row = new List<TableCellDTO>(), };
                    for (int i = 0; i < headersArray.Count(); ++i)
                    {
                        var tableCellDTO = TableCellDTO.Create((i + 1).ToString(), headersArray[i]);
                        headerTableRowDTO.Row.Add(tableCellDTO);
                    }
                    listOfRows.Insert(0, headerTableRowDTO);
                }

                // Process each item in the dictionary and add it as an item in List<TableRowDTO>
                foreach (var row in rowsDictionary.Keys)
                {
                    var listOfCells = new List<TableCellDTO>();
                    foreach (var columnCellValuePair in rowsDictionary[row])
                    {
                        listOfCells.Add(
                            new TableCellDTO()
                            {
                                Cell = new FieldDTO()
                                {
                                    Key = columnCellValuePair.Item1, // Column number
                                    Value = columnCellValuePair.Item2 // Column/cell value
                                }
                            }
                        );
                    }
                    listOfRows.Add(new TableRowDTO() { Row = listOfCells });
                }
                return listOfRows;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
	}
}
