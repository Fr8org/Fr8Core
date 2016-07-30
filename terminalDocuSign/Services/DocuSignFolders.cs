using DocuSign.eSign.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using DocuSign.eSign.Model;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    public static class DocuSignFolders
    {
        public static IEnumerable<KeyValueDTO> GetFolders(DocuSignApiConfiguration conf)
        {
            var api = new FoldersApi(conf.Configuration);
            var folders = api.List(conf.AccountId);
            return folders.Folders?.Where(a => a.Filter == null).Select(a => new KeyValueDTO(a.Name, a.FolderId)) ?? new List<KeyValueDTO>();
        }

        public static IEnumerable<FolderItem> GetFolderItems(DocuSignApiConfiguration config, DocuSignQuery docuSignQuery)
        {
            var resultItems = new List<FolderItem>();

            FoldersApi api = new FoldersApi(config.Configuration);

            if (string.IsNullOrEmpty(docuSignQuery.Folder))
            {
                //return all envelopes from all folders
                var folders = api.List(config.AccountId).Folders.Where(a => a.Filter == null);
                if (folders != null)
                {
                    foreach (var item in folders)
                    {
                        var envelopesResponse = api.ListItems(config.AccountId, item.FolderId,
                            new FoldersApi.SearchOptions()
                            {
                                status = docuSignQuery.Status,
                                searchText = docuSignQuery.SearchText
                            });
                        resultItems.AddRange(envelopesResponse.FolderItems);
                    }
                }
            }
            else
            {
                var envelopesResponse = api.ListItems(config.AccountId, docuSignQuery.Folder,
                    new FoldersApi.SearchOptions()
                    {
                        status = docuSignQuery.Status,
                        searchText = docuSignQuery.SearchText
                    });
                resultItems.AddRange(envelopesResponse.FolderItems);
            }

            return resultItems;
        }

        #region GenerateDocuSignReport methods

        public static int CountEnvelopes(DocuSignApiConfiguration config, DocuSignQuery docuSignQuery)
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