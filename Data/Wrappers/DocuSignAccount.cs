using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Data.Wrappers
{
    public class DocuSignAccount : DocuSign.Integrations.Client.Account
    {
        public static DocuSignAccount Create (Account account)
        {
            return AutoMapper.Mapper.Map<DocuSignAccount>(account);
        }
    }
}
