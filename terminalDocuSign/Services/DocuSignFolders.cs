using DocuSign.eSign.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using DocuSign.eSign.Model;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    public static class DocuSignFolders
    {
        public static IEnumerable<FieldDTO> GetFolders(DocuSignApiConfiguration conf)
        {
            var api = new FoldersApi(conf.Configuration);
            var folders = api.List(conf.AccountId);
            return folders.Folders?.Select(a => new FieldDTO(a.Name, a.FolderId)) ?? new List<FieldDTO>();
        }

        public static IEnumerable<FolderItem> GetFolderItems(DocuSignApiConfiguration config, DocusignQuery docusignQuery)
        {
            var resultItems = new List<FolderItem>();

            FoldersApi api = new FoldersApi(config.Configuration);

            if (string.IsNullOrEmpty(docusignQuery.Folder))
            {
                //return all envelopes from all folders
                var folders = api.List(config.AccountId);
                if (folders.Folders != null)
                {
                    foreach (var item in folders.Folders)
                    {
                        var envelopesResponse = api.ListItems(config.AccountId, item.FolderId,
                            new FoldersApi.SearchOptions()
                            {
                                status = docusignQuery.Status,
                                searchText = docusignQuery.SearchText
                            });
                        resultItems.AddRange(envelopesResponse.FolderItems);
                    }
                }
            }
            else
            {
                var envelopesResponse = api.ListItems(config.AccountId, docusignQuery.Folder,
                    new FoldersApi.SearchOptions()
                    {
                        status = docusignQuery.Status,
                        searchText = docusignQuery.SearchText
                    });
                resultItems.AddRange(envelopesResponse.FolderItems);
            }

            return resultItems;
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