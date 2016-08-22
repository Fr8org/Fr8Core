using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Interfaces
{
    public interface IDocuSignFolder
    {
        List<DocusignFolderInfo> GetFolders(string login, string password);
        List<DocusignFolderInfo> GetSearchFolders(string login, string password);
        List<FolderItem> Search(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null, IEnumerable<FilterConditionDTO> conditions = null);
        int Count(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}