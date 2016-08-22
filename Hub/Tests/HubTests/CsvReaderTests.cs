using System;
using NUnit.Framework;
using System.IO;

using System.Linq;
using Fr8.Testing.Unit;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;


namespace HubTests
{
	[TestFixture]
	[Category("CsvReader")]
	public class CsvReaderTests : BaseTest
	{

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
		}

		[Test]
		public void Ctor_TxtReaderIsNull_ExpectedArgumentNullException()
		{
			TextReader textReader = null;

			var ex = Assert.Throws<ArgumentNullException>(() => new CsvReader(textReader));

			Assert.AreEqual("txtReader", ex.ParamName);
		}
		[Test]
		public void Ctor_FilePathIsNull_ExpectedArgumentNullException()
		{
			string filePath = null;

			var ex = Assert.Throws<ArgumentNullException>(() => new CsvReader(filePath));

			Assert.AreEqual("path", ex.ParamName);
		}
		[Test]
		public void Ctor_FilePathDoesntExist_ExpectedArgumentNullException()
		{
			string filePath = "C:\\" + Guid.NewGuid().ToString() + ".abcdef";

			var ex = Assert.Throws<FileNotFoundException>(() => new CsvReader(filePath));

			Assert.AreEqual(filePath, ex.FileName);
		}
		[Test]
		public void GetColumnHeaders_StreamIsEmtpy_ExpectedInvalidDataException()
		{
			using (var memSteam = new MemoryStream(new byte[0]))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var ex = Assert.Throws<InvalidDataException>(() => csvReader.GetColumnHeaders());

					Assert.AreEqual("There is no data in stream", ex.Message);
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContainsOnlyCLRF_ExpectedInvalidDataException()
		{
			var clrfBytes = System.Text.Encoding.UTF8.GetBytes("\r\n");
			using (var memSteam = new MemoryStream(clrfBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var ex = Assert.Throws<InvalidDataException>(() => csvReader.GetColumnHeaders());

					Assert.AreEqual("There is no data in stream", ex.Message);
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamDoesntContainsCLRF_ExpectedOneColumn()
		{
			string expectedColumnName = "BLABLABLA";
			var clrfBytes = System.Text.Encoding.UTF8.GetBytes(expectedColumnName);
			using (var memSteam = new MemoryStream(clrfBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(1, columns.Length, "Expected 1 column");
					Assert.Contains(expectedColumnName, columns, "Expected 1 column with name '{0}'".format(expectedColumnName));
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains2Columns_Expected2Columns()
		{
			var expectedColumns = GenerateColumns(2);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains3Columns_Expected3Columns()
		{
			var expectedColumns = GenerateColumns(3);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains4Columns_Expected4Columns()
		{
			var expectedColumns = GenerateColumns(4);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains5Columns_Expected5Columns()
		{
			var expectedColumns = GenerateColumns(5);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains100Columns_Expected100Columns()
		{
			var expectedColumns = GenerateColumns(100);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_DoubleCall_ShouldBeOk()
		{
			var expectedColumns = GenerateColumns(100);
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join(",", expectedColumns));
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns1 = csvReader.GetColumnHeaders();
					var columns2 = csvReader.GetColumnHeaders();

					Assert.AreEqual(expectedColumns.Length, columns1.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns1, "Doesn't expect differencies");
					Assert.AreEqual(expectedColumns.Length, columns2.Length, "Expected {0} columns", expectedColumns.Length);
					Assert.AreEqual(expectedColumns, columns2, "Doesn't expect differencies");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContainsEmtpyAndNonEmptyColumns_Expected8Columns()
		{
			var expectedColumn = "A,,B,,C,,D,";
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(expectedColumn);
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(8, columns.Length, "Expected 8 columns");
					Assert.AreEqual("A", columns[0], "Expected 'A' in the 1 column");
					Assert.AreEqual("", columns[1], "Expected '' in the 2 column");
					Assert.AreEqual("B", columns[2], "Expected 'B' in the 3 column");
					Assert.AreEqual("", columns[3], "Expected '' in the 4 column");
					Assert.AreEqual("C", columns[4], "Expected 'C' in the 5 column");
					Assert.AreEqual("", columns[5], "Expected '' in the 6 column");
					Assert.AreEqual("D", columns[6], "Expected 'D' in the 7 column");
					Assert.AreEqual("", columns[7], "Expected '' in the 8 column");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains2EmtpyColumns_Expected2EmtpyColumns()
		{
			var expectedColumn = ",";
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(expectedColumn);
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(2, columns.Length, "Expected 2 columns");
					Assert.AreEqual("", columns[0], "Expected '' in the 1 column");
					Assert.AreEqual("", columns[1], "Expected '' in the 2 column");
				}
			}
		}
		[Test]
		public void GetColumnHeaders_StreamContains3EmtpyColumns_Expected3EmtpyColumns()
		{
			var expectedColumn = ",,";
			var columnsAsBytes = System.Text.Encoding.UTF8.GetBytes(expectedColumn);
			using (var memSteam = new MemoryStream(columnsAsBytes))
			{
				using (StreamReader sm = new StreamReader(memSteam))
				{
					ICsvReader csvReader = new CsvReader(sm);

					var columns = csvReader.GetColumnHeaders();

					Assert.AreEqual(3, columns.Length, "Expected 3 columns");
					Assert.AreEqual("", columns[0], "Expected '' in the 1 column");
					Assert.AreEqual("", columns[1], "Expected '' in the 2 column");
					Assert.AreEqual("", columns[2], "Expected '' in the 3 column");
				}
			}
		}
		private string[] GenerateColumns(int cnt)
		{
			if (cnt <= 0)
				throw new ArgumentOutOfRangeException("cnt", "cnt must be >= 1");
			string[] result = new string[cnt];
			for (int i = 0; i < cnt; i++)
			{
				result[i] = "field_{0}".format(i + 1);
			}
			return result;
		}
	}
}
