using System.Collections.Generic;
using DocuSign.eSign.Model;
using fr8.Infrastructure.Data.DataTransferObjects;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    public class DocuSignFoldersWrapper : IDocuSignFolders
    {
        public IEnumerable<FieldDTO> GetFolders(DocuSignApiConfiguration conf)
        {
            return DocuSignFolders.GetFolders(conf);
        }

        public IEnumerable<FolderItem> GetFolderItems(DocuSignApiConfiguration config, DocuSignQuery docuSignQuery)
        {
            return DocuSignFolders.GetFolderItems(config, docuSignQuery);
        }
    }
}