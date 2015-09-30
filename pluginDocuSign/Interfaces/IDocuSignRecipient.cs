using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
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
		Recipients GetByTemplate(string templateId);
	}
}
