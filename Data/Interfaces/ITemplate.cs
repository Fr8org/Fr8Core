using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.Integrations.Client;
using Utilities;
using Data.Wrappers;

namespace Data.Interfaces
{
    public interface ITemplate
    {
        List<string> GetMappableSourceFields(DocuSignEnvelope envelop);
        IEnumerable<string> GetMappableSourceFields(int templateId);
    }
}
