using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
	public interface IGoogleDrive
	{
	    Task<DriveService> CreateDriveService(GoogleAuthDTO authDTO);
	    bool FileExist(DriveService driveService, string filename, out string link);
	    Task<Dictionary<string, string>> GetGoogleForms(GoogleAuthDTO authDTO);
        Task<Google.Apis.Drive.v2.Data.File> CreateFile(string title, byte[] body, string mimeType, GoogleAuthDTO authDTO);

    }
}