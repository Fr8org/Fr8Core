using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using DocuSign.eSign.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using DocuSign.eSign.Model;
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

        public static void SearchDocuSign(DocuSignApiConfiguration config, DocusignQuery docusignQuery, StandardPayloadDataCM search_result)
        {
            var resultItems = new List<DocuSign.eSign.Model.FolderItem>();

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

            //prepare search result
            foreach (var envelope in resultItems)
            {
                var row = new PayloadObjectDTO();

                row.PayloadObject.Add(new FieldDTO("Id", envelope.EnvelopeId));
                row.PayloadObject.Add(new FieldDTO("Name", envelope.Name));
                row.PayloadObject.Add(new FieldDTO("Subject", envelope.Subject));
                row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
                row.PayloadObject.Add(new FieldDTO("OwnerName", envelope.OwnerName));
                row.PayloadObject.Add(new FieldDTO("SenderName", envelope.SenderName));
                row.PayloadObject.Add(new FieldDTO("SenderEmail", envelope.SenderEmail));
                row.PayloadObject.Add(new FieldDTO("Shared", envelope.Shared));
                row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDateTime?.ToString(CultureInfo.InvariantCulture)));
                row.PayloadObject.Add(new FieldDTO("CreatedDateTime", envelope.CreatedDateTime?.ToString(CultureInfo.InvariantCulture)));

                search_result.PayloadObjects.Add(row);
            }
        }

        #endregion
    }
}