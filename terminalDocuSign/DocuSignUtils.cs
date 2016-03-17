using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace terminalDocuSign
{
	class DocuSignUtils
	{
		public static void ThrowInvalidOperationExceptionIfError(JObject jObjTemplate)
		{
			Error error = Error.FromJson(jObjTemplate.ToString());
			if (!string.IsNullOrEmpty(error.errorCode))
				throw new InvalidOperationException("ErrorCode: {0}. Message: {1}".format(error.errorCode, error.message));
		}
	}
}
