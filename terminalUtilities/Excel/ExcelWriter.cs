using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace terminalUtilities.Excel
{
    public class ExcelWriter
    {
        public void WriteFile(Stream fileStream, DataTable data)
        {
            using (var doc = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook))
            {
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                doc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                var ssTablePart = EnsureSharedStringTablePart(doc);
                var wsPart = CreateWorksheet(doc.WorkbookPart, data.TableName);

                var stylesPart = doc.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();

                SaveDataToWorksheet(data, wsPart, ssTablePart);
                // doc.Validate(null);
            }
        }

        public void RewriteSheetForFile(Stream stream, DataTable data)
        {
            using (var doc = SpreadsheetDocument.Open(stream, true))
            {
                var ssTablePart = EnsureSharedStringTablePart(doc);

                var wsPart = GetWorksheet(doc.WorkbookPart, data.TableName);
                if (wsPart != null)
                {
                    ClearWorksheet(wsPart);
                }
                else
                {
                    wsPart = CreateWorksheet(doc.WorkbookPart, data.TableName);

                    // var stylesPart = doc.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    // stylesPart.Stylesheet = new Stylesheet();
                }

                SaveDataToWorksheet(data, wsPart, ssTablePart);
                // doc.Validate(null);
            }
        }

        private SharedStringTablePart EnsureSharedStringTablePart(SpreadsheetDocument doc)
        {
            if (doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                return doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                return doc.WorkbookPart.AddNewPart<SharedStringTablePart>();
            }
        }

        private void SaveDataToWorksheet(DataTable data, WorksheetPart wsPart, SharedStringTablePart ssTablePart)
        {
            var ssTableIndex = new Dictionary<string, int>();
            var rowsIndex = new Dictionary<uint, Row>();

            uint rowIndex = 1;

            // Header.
            int columnIndex = 0;
            foreach (DataColumn col in data.Columns)
            {
                var sharedStringIndex = InsertSharedStringItem(ssTablePart, col.ColumnName, ssTableIndex);
                var columnReference = GetExcelColumnReference(columnIndex);

                var cell = InsertCellInWorksheet(
                    columnReference,
                    rowIndex,
                    wsPart,
                    rowsIndex);

                cell.CellValue = new CellValue(sharedStringIndex.ToString());
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                ++columnIndex;
            }

            ++rowIndex;

            // Data.
            foreach (DataRow row in data.Rows)
            {
                columnIndex = 0;
                foreach (DataColumn col in data.Columns)
                {
                    var value = row[col.ColumnName];
                    var valueStr = GetStringValue(value);

                    var sharedStringIndex = InsertSharedStringItem(ssTablePart, valueStr, ssTableIndex);
                    var columnReference = GetExcelColumnReference(columnIndex);

                    var cell = InsertCellInWorksheet(
                        columnReference,
                        rowIndex,
                        wsPart,
                        rowsIndex);

                    cell.CellValue = new CellValue(sharedStringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);

                    ++columnIndex;
                }

                ++rowIndex;
            }

            wsPart.Worksheet.Save();
        }

        private string GetStringValue(object value)
        {
            string valueStr;

            if (value == null)
            {
                valueStr = "";
            }
            else if (value.GetType() == typeof(float))
            {
                valueStr = ((double)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value.GetType() == typeof(double))
            {
                valueStr = ((double)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value.GetType() == typeof(decimal))
            {
                valueStr = ((decimal)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value.GetType() == typeof(int))
            {
                valueStr = ((int)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value.GetType() == typeof(long))
            {
                valueStr = ((long)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (value.GetType() == typeof(DateTime))
            {
                valueStr = ((DateTime)value).ToString("dd.MM.yyyy HH:mm:ss");
            }
            else
            {
                valueStr = value.ToString();
            }

            return valueStr;
        }

        private WorksheetPart GetWorksheet(WorkbookPart wbPart, string sheetName)
        {
            var sheets = wbPart.Workbook.GetFirstChild<Sheets>();
            var sheet = sheets.Elements<Sheet>().FirstOrDefault(x => x.Name == sheetName);

            if (sheet == null)
            {
                return null;
            }

            var wsPart = wbPart.GetPartsOfType<WorksheetPart>()
                .FirstOrDefault(x => wbPart.GetIdOfPart(x) == sheet.Id);

            return wsPart;
        }

        private WorksheetPart CreateWorksheet(WorkbookPart wbPart, string sheetName)
        {
            var sheets = wbPart.Workbook.GetFirstChild<Sheets>();

            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            wsPart.Worksheet = new Worksheet(new SheetData());
            wsPart.Worksheet.Save();

            var relationshipId = wbPart.GetIdOfPart(wsPart);

            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(x => x.SheetId.Value).Max() + 1;
            }

            var sheet = new Sheet()
            {
                Id = relationshipId,
                SheetId = sheetId,
                Name = sheetName
            };
            sheets.Append(sheet);
            wbPart.Workbook.Save();

            return wsPart;
        }

        private int InsertSharedStringItem(SharedStringTablePart ssTablePart,
            string text, Dictionary<string, int> ssTableIndex)
        {
            if (ssTablePart.SharedStringTable == null)
            {
                ssTablePart.SharedStringTable = new SharedStringTable();
            }

            int index;
            if (ssTableIndex.TryGetValue(text, out index))
            {
                return index;
            }

            ssTablePart.SharedStringTable.AppendChild(
                new SharedStringItem(
                    new DocumentFormat.OpenXml.Spreadsheet.Text(text)
                )
            );

            index = ssTableIndex.Count;
            ssTableIndex.Add(text, index);

            return index;
        }

        private Cell InsertCellInWorksheet(string columnName,
            uint rowIndex, WorksheetPart wsPart, Dictionary<uint, Row> rowsIndex)
        {
            var worksheet = wsPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            var cellReference = columnName + rowIndex.ToString();

            Row row;
            if (!rowsIndex.TryGetValue(rowIndex, out row))
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);

                rowsIndex.Add(rowIndex, row);
            }

            var newCell = new Cell() { CellReference = cellReference };
            row.Append(newCell);

            return newCell;
        }

        private void ClearWorksheet(WorksheetPart wsPart)
        {
            var worksheet = wsPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();

            var rows = sheetData.Descendants<Row>().ToList();
            foreach (var row in rows)
            {
                row.Remove();
            }
        }

        private static string GetExcelColumnReference(int index)
        {
            int alphabetCount = 'Z' - 'A' + 1;
            var sb = new StringBuilder();

            while (index >= alphabetCount)
            {
                sb.Insert(0, (char)('A' + (index % alphabetCount)));
                index = index / alphabetCount - 1;
            }

            sb.Insert(0, (char)('A' + (index % alphabetCount)));

            return sb.ToString();
        }
    }
}
