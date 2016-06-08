using System;
using System.Collections.Generic;
using Data.Interfaces;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalDocuSign.Services.MT
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