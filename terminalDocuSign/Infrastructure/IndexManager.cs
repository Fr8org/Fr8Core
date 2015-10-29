using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.ManifestSchemas;
using Data.Repositories;
using Hub.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;

namespace terminalDocuSign.Infrastructure
{
    public class IndexManager
    {
        public AuthorizationTokenDO currentAuthToken;
        public void IndexDocuSignUser(AuthorizationTokenDO curAuthToken, DateTime startDate)
        {
            currentAuthToken = curAuthToken;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var parsingRecords = uow.MultiTenantObjectRepository.Get<StandardParsingRecord>(uow, curAuthToken.UserID, a => a.InternalAccountId == curAuthToken.UserID && a.ExternalAccountId == curAuthToken.ExternalAccountId);
            }
        }

        public void QueryParse(DateTime startDate,DateTime endDate)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthDTO>(currentAuthToken.Token);
            int resultCount = 100;

            DocuSignEnvelope docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);

           var accountEnvelopes = docusignEnvelope.GetEnvelopes("completed", startDate, endDate, true, resultCount);

        }
    }
}