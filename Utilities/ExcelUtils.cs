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
	}
}
