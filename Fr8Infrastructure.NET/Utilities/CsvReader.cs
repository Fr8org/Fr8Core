using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fr8.Infrastructure.Interfaces;

namespace Fr8.Infrastructure.Utilities
{
	public class CsvReader : ICsvReader
	{
		private readonly TextReader _txtReader;
		private readonly char _delimeter;
		private readonly bool _txtReaderCreatedHere;
		private string[] _headers;
        private Dictionary<string, List<Tuple<string, string>>> _tabularData;
		public string FilePath { get; private set; }

		public CsvReader(string path, char delimeter = ',')
			: this(System.IO.File.OpenText(path), delimeter)
		{
			FilePath = path;
			_txtReaderCreatedHere = true;
		}
		public CsvReader(System.IO.TextReader txtReader, char delimeter = ',')
		{
			if (txtReader == null)
				throw new ArgumentNullException("txtReader");
			
			_txtReader = txtReader;
			_delimeter = delimeter;
		}
		
        public string[] GetColumnHeaders()
		{
			if (_headers == null)
			{
				// From https://tools.ietf.org/html/rfc4180
				// Each field may or may not be enclosed in double quotes (however some programs, such as Microsoft Excel, do not use double quotes at all).
				// We will not process double quotes fields
				string firstLine = _txtReader.ReadLine();
				if (string.IsNullOrEmpty(firstLine))
					throw new InvalidDataException("There is no data in stream");

				_headers = firstLine.Split(new char[] { _delimeter }, StringSplitOptions.None);
			}
			return _headers;
		}

        public Dictionary<string, List<Tuple<string, string>>> GetTabularData()
        {
            _headers = GetColumnHeaders(); // this call will make sure that the first line has been read;
            List<string> headerList = _headers.ToList();

            if (_tabularData == null)
            {
                _tabularData = new Dictionary<string, List<Tuple<string, string>>>();
                int rowNumber = 0;
                while (true)
                {
                    ++rowNumber;
                    string nextLine = _txtReader.ReadLine();
                    if (string.IsNullOrEmpty(nextLine.Replace(",", "")))
                        break;

                    var dataValues = nextLine.Split(new char[] { _delimeter }, StringSplitOptions.None);
                    var listOfDataCellsForARow = new List<Tuple<string, string>>();
                    for(int i = 0; i < headerList.Count; ++i)
                    {
                         listOfDataCellsForARow.Add(new Tuple<string, string>(headerList[i], dataValues[i]));
                    }

                    _tabularData.Add(rowNumber.ToString(), listOfDataCellsForARow);
                }
            }
            return _tabularData;
        }

		public void Dispose()
		{
			if (_txtReaderCreatedHere)
			{
				try { _txtReader.Dispose(); }
				catch { }
			}
		}
	}
}
