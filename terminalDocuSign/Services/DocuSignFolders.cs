using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using DocuSign.eSign.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    public static class DocuSignFolders
    {
        public static IEnumerable<FieldDTO> GetFolders(DocuSignApiConfiguration conf)
        {
            FoldersApi api = new FoldersApi(conf.Configuration);
            var folders = api.List(conf.AccountId);
            if (folders.Folders != null)
                return folders.Folders.Select(a => new FieldDTO(a.Name, a.Name));
            else
                return new List<FieldDTO>();
        }

        #region GenerateDocuSignReport methods

        public static int CountEnvelopes(DocuSignApiConfiguration config, DocusignQuery docusignQuery)
        {
            throw new NotImplementedException();
        }

        public static object SearchDocuSign(DocuSignApiConfiguration config, List<FilterConditionDTO> conditions, HashSet<string> existing_envelopes, StandardPayloadDataCM search_result)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}