using System;

namespace Hub.Interfaces
{
	public interface ITime
	{
		DateTime CurrentDateTime();
		DateTimeOffset CurrentDateTimeOffset();
	}
}