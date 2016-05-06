using System;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace TerminalBase.Services
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