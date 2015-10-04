using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Interfaces
{
	public interface ICsvReader : IDisposable
	{
		string[] GetColumnHeaders();
        Dictionary<string, List<Tuple<string, string>>> GetTabularData();
	}
}
