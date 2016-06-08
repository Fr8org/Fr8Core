using System.Collections.Generic;
using DocuSign.eSign.Model;
using fr8.Infrastructure.Data.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Interfaces
{
    public interface IDocuSignFolders
    {
        IEnumerable<FieldDTO> GetFolders(DocuSignApiConfiguration conf);

        IEnumerable<FolderItem> GetFolderItems(DocuSignApiConfiguration config, DocuSignQuery docuSignQuery);
    }
}