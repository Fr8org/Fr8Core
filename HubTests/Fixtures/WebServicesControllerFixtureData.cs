using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubTests.Fixtures
{
    public partial class FixtureData
    {
        public static WebServiceDO BasicWebServiceDO()
        {
            var webServiceDO = new WebServiceDO { Name = "IntegrationTestWebService", IconPath = "IntegrationTestIconPath", Id = 1 };

            return webServiceDO;
        }

        public static WebServiceDTO BasicWebServiceDTOWithoutId()
        {
            var webServiceDTO = new WebServiceDTO { Name = "IntegrationTestWebService", IconPath = "IntegrationTestIconPath" };

            return webServiceDTO;
        }
    }
}
