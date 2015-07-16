using KwasantTest;
using NUnit.Framework;
using System;
using pluginGoogleSheet;
using System.Collections.Generic;

namespace pluginGoogleSheetTestsTests
{
    [TestFixture]
    public class SampleControllerTest : BaseTest
    {
        [Test]
        [Category("Plugin.GoogleSheet")]
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