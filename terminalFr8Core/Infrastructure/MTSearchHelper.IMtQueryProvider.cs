using System;
using System.Collections.Generic;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace terminalFr8Core.Infrastructure
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