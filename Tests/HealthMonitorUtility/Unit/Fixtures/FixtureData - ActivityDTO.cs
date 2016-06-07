using System;
using Fr8Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
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

        public static ActivityDTO Get_Google_Sheet_Data_v1_InitialConfiguration()
        {
            var activityTemplate = GetGoogleSheetDataActivityTemplate();
            return new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Get Google Sheet Data",
                AuthToken = GetGoogleAuthorizationToken(),
                ActivityTemplate = activityTemplate
            };
        }

        public static ActivityDTO Monitor_Form_Responses_v1_InitialConfiguration()
        {
            var activityTemplate = MonitorFormResponsesActivityTemplate();
            return new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = GetGoogleAuthorizationToken(),
                ActivityTemplate = activityTemplate
            };
        }
        public static ActivityDTO Build_Message_v1_InitialConfiguration()
        {
            var activityTemplate = BuildMessageActivityTemplate();
            return new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Build Message",
                ActivityTemplate = activityTemplate
            };
        }

        public static ActivityDTO Get_File_List_v1_InitialConfiguration()
        {
            var activityTemplate = GetFileListActivityTemplate();
            return new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Get File List",
                ActivityTemplate = activityTemplate
            };
        }
    }
}
