using Data.Interfaces.DataTransferObjects;
using pluginDocuSign.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pluginDocuSign.Interfaces
{
	public interface IDocuSignRecipient
	{
		RecipientsDTO GetByTemplate(string templateId);
	}
}
