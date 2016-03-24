using System.Collections.Generic;
using Data.Interfaces.Manifests;
using terminalGoogle.DataTransferObjects;
using Google.GData.Spreadsheets;
using System.Threading.Tasks;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleSheet
    {
        Task<Dictionary<string, string>> GetSpreadsheetsAsync(GoogleAuthDTO authDTO);

        Task<Dictionary<string, string>> GetWorksheetsAsync(string spreadsheetUri, GoogleAuthDTO authDTO);

        Task<Dictionary<string, string>> GetWorksheetHeadersAsync(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);

        Task<IEnumerable<TableRowDTO>> GetDataAsync(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);
        Dictionary<string, string> EnumerateSpreadsheetsUris(GoogleAuthDTO authDTO);
        Dictionary<string, string> EnumerateColumnHeaders(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);
        IEnumerable<TableRowDTO> EnumerateDataRows(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO);

        /// <summary>
        /// Exports Google Spreadsheet data to CSV file and save it on storage
        /// </summary>
        /// <param name="spreadsheetUri">Spreadsheet URI</param>
        /// <returns>Returns a link to CSV file on storage</returns>
        string ExtractData(string spreadsheetUri, GoogleAuthDTO authDTO);
        Dictionary<string, string> EnumerateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO);
        string CreateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname);
        SpreadsheetEntry FindSpreadsheet(string spreadsheetUri, GoogleAuthDTO authDTO);
        Task<string> CreateSpreadsheet(string spreadsheetname, GoogleAuthDTO authDTO);
        bool WriteData(string spreadsheetUri, string worksheetUri, StandardTableDataCM data, GoogleAuthDTO authDTO);
    }
}