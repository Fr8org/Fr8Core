using System;
using Hub.Interfaces;

namespace Hub.Services
{
	public class Time : ITime
	{
		public DateTime CurrentDateTime()
		{
			return DateTime.UtcNow;
		}

		public DateTimeOffset CurrentDateTimeOffset()
		{
			return DateTimeOffset.UtcNow;
		}
	}
}