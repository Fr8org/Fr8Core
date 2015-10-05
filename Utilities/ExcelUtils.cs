using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
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
        public static Dictionary<string, List<Tuple<string, string>>> GetTabularData(byte[] fileBytes, string extension)
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
                    excelReader.IsFirstRowAsColumnNames = true;
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
	}
}
