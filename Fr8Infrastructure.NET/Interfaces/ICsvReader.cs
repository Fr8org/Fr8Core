using System;
using System.Collections.Generic;

namespace fr8.Infrastructure.Interfaces
{
	public interface ICsvReader : IDisposable
	{
		string[] GetColumnHeaders();
        Dictionary<string, List<Tuple<string, string>>> GetTabularData();
	}
}
