using System.Collections.Generic;
using terminalGoogle.DataTransferObjects;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleSheet
    {
        Task<Dictionary<string, string>> GetSpreadsheets(GoogleAuthDTO authDTO);

        Task<Dictionary<string, string>> GetWorksheets(string spreadsheetUri, GoogleAuthDTO authDTO);

        Task<Dictionary<string, string>> GetWorksheetHeaders(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);

        Task<IEnumerable<TableRowDTO>> GetData(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);

        Task<string> CreateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname);

        Task<string> CreateSpreadsheet(string spreadsheetname, GoogleAuthDTO authDTO);

        Task WriteData(string spreadsheetUri, string worksheetUri, StandardTableDataCM data, GoogleAuthDTO authDTO);

        Task DeleteSpreadSheet(string spreadsheetname, GoogleAuthDTO authDTO);

        Task DeleteWorksheet(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);
    }
}