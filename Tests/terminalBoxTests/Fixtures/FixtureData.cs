using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Ploeh.AutoFixture;

namespace terminalBoxTests.Fixtures
{
    public class FixtureData
    {
        private static readonly Fixture Fixture;

        static FixtureData()
        {
            // AutoFixture Setup
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static Fr8DataDTO SaveToFileTestAction()
        {
            ActivityTemplateSummaryDTO activityTemplateDto = Fixture.Build<ActivityTemplateSummaryDTO>()
               .With(x => x.Name, "SaveToFile_TEST")
               .With(x => x.Version, "1")
               .OmitAutoProperties()
               .Create();

            ActivityDTO activityDto = Fixture.Build<ActivityDTO>()
               .With(x => x.Id)
               .With(x => x.ActivityTemplate, activityTemplateDto)
               .With(x => x.AuthToken, GetBoxAuthToken())
               .OmitAutoProperties()
               .Create();

            return Fixture.Build<Fr8DataDTO>()
                .With(x => x.ActivityDTO, activityDto)
                .OmitAutoProperties()
                .Create();
        }

        public static AuthorizationTokenDTO GetBoxAuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token =
                    "{\"AccessToken\": \"YTF4KdbW2RGPFqaUPqRH5W4Dlb6fU1k1\", \"RefreshToken\": \"z5bPuVGxrtBYmjDJBrPpmqSIOmkIO4AHmLpCUQjEF4z9Qph8Vka1ojgpzQRamN36\", \"ExpiresAt\": \"2016 - 05 - 11T15:12:11.8109357Z\" }"
            };
        }

        public static StandardTableDataCM GetStandardTableDataCM()
        {
            return new StandardTableDataCM();
        }
    }
}