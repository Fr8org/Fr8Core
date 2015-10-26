using System.Collections.Generic;
using Data.Interfaces.ManifestSchemas;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleSheet
    {
        Dictionary<string, string> EnumerateSpreadsheetsUris(GoogleAuthDTO authDTO);
        IEnumerable<string> EnumerateColumnHeaders(string spreadsheetUri, GoogleAuthDTO authDTO);
        IEnumerable<TableRowDTO> EnumerateDataRows(string spreadsheetUri, GoogleAuthDTO authDTO);

        /// <summary>
        /// Exports Google Spreadsheet data to CSV file and save it on storage
        /// </summary>
        /// <param name="spreadsheetUri">Spreadsheet URI</param>
        /// <returns>Returns a link to CSV file on storage</returns>
        string ExtractData(string spreadsheetUri, GoogleAuthDTO authDTO);

        string CreateOAuth2AuthorizationUrl();
        GoogleAuthDTO GetToken(string code);
    }
}