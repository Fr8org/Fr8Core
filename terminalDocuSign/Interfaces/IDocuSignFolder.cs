using System;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Interfaces
{
    public interface IDocuSignFolder
    {
        DocusignFolderInfo[] GetFolders(string login, string password);
        FolderItem[] Search(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}