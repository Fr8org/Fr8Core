using System;
using System.Net.Http;
using Core.Interfaces;
using Core.Services;
using Data.States.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    public class EventTest: BaseTest
    {
        
        private IEvent _event;
        [SetUp]
        //constructor method as it is run at the test start
        public override void SetUp()
        {
            base.SetUp();

            _event = ObjectFactory.GetInstance<IEvent>();
        }

        #region 

        [TestMethod]
        public void Event_RequestParsingFromPlugins_ReturnsOk()
        {
            var curPlugin = FixtureData.PluginOne();

            var mockHttpClient = new Mock<HttpClient>();

            //mockHttpClient.Setup(c => c.PostAsync())



           // var result = await _event.RequestParsingFromPlugins(, curPlugin.Name, curPlugin.Version);

          //  Assert.AreEqual(result, "ok");
        }

        #endregion

    }
}
