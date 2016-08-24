using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
	public interface IGoogleDrive
	{
	    Task<DriveService> CreateDriveService(GoogleAuthDTO authDTO);
	    bool FileExist(DriveService driveService, string filename, out string link);
	    Task<Dictionary<string, string>> GetGoogleForms(GoogleAuthDTO authDTO);
	}
}