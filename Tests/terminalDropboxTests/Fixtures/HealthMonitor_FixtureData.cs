using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalDropboxTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static Guid TestGuid_Id_333()
        {
            return new Guid("8339DC87-F011-4FB1-B47C-FEC406E4100A");
        }

        public static Fr8DataDTO GetFileListTestFr8DataDTO()
        {
            var actionTemplate = GetFileListTestActivityTemplateDTO();

            var activityDTO = new ActivityDTO()
            {
                Id = TestGuid_Id_333(),
                ActivityTemplate = actionTemplate,
                CrateStorage = null,
                AuthToken = DropboxAuthorizationTokenDTO()

            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
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
            return new AuthorizationTokenDTO()
            {
                Token = "bLgeJYcIkHAAAAAAAAAAFf6hjXX_RfwsFNTfu3z00zrH463seBYMNqBaFpbfBmqf"
            };
        }
    }
}
