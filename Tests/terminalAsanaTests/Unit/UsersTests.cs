using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Testing.Unit;
using Moq;
using NUnit.Common;
using NUnit.Framework;
using terminalAsana.Asana;
using terminalAsana.Interfaces;

namespace terminalAsanaTests.Unit
{
    [Category("terminalAsana")]
    class UsersTests: BaseTest
    {
        private IAsanaUsers _asanaUsersService;

        [OneTimeSetUp]
        public void StartUp()
        {
            var httpClient = new Mock<IRestfulServiceClient>().Object;
            _asanaUsersService = new Users(httpClient);
        }

        [Test]
        public void Should_Get_Me_Info()
        {
            var token = "tokenString";

        }
        
    }
}
