using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Ploeh.AutoFixture;

namespace terminalDropboxTests.Fixtures
{
    public static class HealthMonitor_FixtureData
    {
        private static readonly Fixture Fixture;

        static HealthMonitor_FixtureData()
        {
            // AutoFixture Setup
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        public static Guid TestGuid_Id_333()
        {
            return new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A");
        }

        public static Fr8DataDTO GetFileListTestFr8DataDTO()
        {
            ActivityTemplateSummaryDTO activityTemplateDto = Fixture.Build<ActivityTemplateSummaryDTO>()
                .With(x => x.Name, "Get_File_List_TEST")
                .With(x => x.Version, "1")
                .OmitAutoProperties()
                .Create();

            ActivityDTO activityDto = Fixture.Build<ActivityDTO>()
                .With(x => x.Id)
                .With(x => x.ActivityTemplate, activityTemplateDto)
                .With(x => x.CrateStorage, null)
                .With(x => x.AuthToken, DropboxAuthorizationTokenDTO())
                .OmitAutoProperties()
                .Create();

            return Fixture.Build<Fr8DataDTO>()
                .With(x => x.ActivityDTO, activityDto)
                .OmitAutoProperties()
                .Create();
        }

        public static ActivityTemplateDTO GetFileListTestActivityTemplateDTO()
        {
            return new ActivityTemplateDTO
            {
                Id = Guid.NewGuid(),
                Name = "Get_File_List_TEST",
                Version = "1"
            };
        }

        public static AuthorizationTokenDTO DropboxAuthorizationTokenDTO()
        {
            return Fixture.Build<AuthorizationTokenDTO>()
                .With(x => x.Token, "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf")
                .OmitAutoProperties()
                .Create();
        }
    }
}
