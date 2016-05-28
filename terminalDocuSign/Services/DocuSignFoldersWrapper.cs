using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocuSign.eSign.Model;
using Fr8Data.DataTransferObjects;
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