using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubTests.Managers
{
    [TestFixture]
    [Category("CrateManager")]
    public class CrateManagerTests
    {
        [Test]
        public void CreateErrorCrate_ReturnsCrateDTO()
        {
            var crateManager = new CrateManager();
            var errorMessage = "This is test error message";
            var result = crateManager.CreateErrorCrate(errorMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual("Retry Crate", result.Label);
            Assert.AreEqual(errorMessage, result.Contents);


        }
    }
}
