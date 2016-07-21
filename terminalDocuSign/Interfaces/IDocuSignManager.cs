using System;
using System.Collections.Generic;
using Data.Entities;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Services.New_Api
{
    public interface IDocuSignManager
    {
        DocuSignApiConfiguration SetUp(DocuSignAuthTokenDTO authToken);
        DocuSignApiConfiguration SetUp(string token);
        DocuSignApiConfiguration SetUp(AuthorizationToken authTokenDO);
        List<KeyValueDTO> GetTemplatesList(DocuSignApiConfiguration conf);
        JObject DownloadDocuSignTemplate(DocuSignApiConfiguration config, string selectedDocusignTemplateId);
        IEnumerable<KeyValueDTO> GetEnvelopeRecipientsAndTabs(DocuSignApiConfiguration conf, string envelopeId);
        IEnumerable<KeyValueDTO> GetTemplateRecipientsAndTabs(DocuSignApiConfiguration conf, string templateId);
        Tuple<IEnumerable<KeyValueDTO>, IEnumerable<DocuSignTabDTO>> GetTemplateRecipientsTabsAndDocuSignTabs(DocuSignApiConfiguration conf, string templateId);
        void SendAnEnvelopeFromTemplate(DocuSignApiConfiguration loginInfo, List<KeyValueDTO> rolesList, List<KeyValueDTO> fieldList, string curTemplateId, StandardFileDescriptionCM file = null);
        bool DocuSignTemplateDefaultNames(IEnumerable<DocuSignTabDTO> templateDefinedFields);
    }
}