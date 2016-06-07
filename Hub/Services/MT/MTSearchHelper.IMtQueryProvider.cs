using System;
using System.Collections.Generic;
using Data.Interfaces;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services
{
	static partial class MTSearchHelper
	{
        public interface IMtQueryProvider
        {
            Type Type { get; }

            List<object> Query(IUnitOfWork uow, string accountId, List<FilterConditionDTO> conditions);
        }
	}
}