using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleAppsScript
    {
        Task<List<GoogleFormField>> GetGoogleFormFields(GoogleAuthDTO authDTO, string formId);
        Task CreateFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string email);
        Task<string> CreateManualFr8TriggerForDocument(GoogleAuthDTO authDTO, string formId, string desription = "Script uploaded from Fr8 application");
    }
}