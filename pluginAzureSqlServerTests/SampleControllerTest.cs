using DockyardTest;
using NUnit.Framework;
using System;
using pluginAzureSqlServer;
using System.Collections.Generic;

namespace pluginAzureSqlServerTests
{
    [TestFixture]
    public class SampleControllerTest : BaseTest
    {
        [Test]
        [Category("Plugin.AzureSqlServer")]
        public void SampleController_ReturnsTestDataOnGetRequest()
        {
            //Arrange
            SampleController controller = new SampleController();

            //Act 
            IEnumerable<string> data = controller.Get();

            //Assert
            IEnumerator<string> e = data.GetEnumerator();
            e.MoveNext();
            Assert.AreEqual("value1", e.Current);
            e.MoveNext();
            Assert.AreEqual("value2", e.Current);
        }
    }
}