using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.Integrations.Client;
using Utilities;

namespace Data.Interfaces
{
    public interface ITemplate
    {
        List<string> GetMappableSourceFields(DocuSign.Integrations.Client.Envelope envelop);
    }
}
