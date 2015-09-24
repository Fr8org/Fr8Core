using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
	public interface IDocuSignRecipient
	{
		RecipientsDTO GetByTemplate(string templateId);
	}
}
