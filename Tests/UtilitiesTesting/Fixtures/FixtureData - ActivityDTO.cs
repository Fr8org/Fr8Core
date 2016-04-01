using System;
using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityDTO Query_DocuSign_v1_InitialConfiguration()
        {
            var activityTemplate = QueryDocuSignActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Query DocuSign",
                ActivityTemplate = activityTemplate
            };

            return activityDTO;
        }
        
        public static ActivityDTO Save_To_Google_Sheet_v1_InitialConfiguration()
        {
            var activityTemplate = SaveToGoogleSheetActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Save To Google Sheet",
                AuthToken = GetGoogleAuthorizationToken(),
                ActivityTemplate = activityTemplate
            };

            return activityDTO;
        }
    }
}
