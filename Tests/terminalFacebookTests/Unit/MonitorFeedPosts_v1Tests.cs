using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalFacebook.Activities;
using Fr8.Testing.Unit;

namespace terminalFacebookTests.Unit
{
    [TestFixture]
    public class MonitorFeedPosts_v1Tests : BaseTest
    {

        public override void SetUp()
        {
            base.SetUp();
            var hubMock = new Mock<IHubCommunicator>();

            ObjectFactory.Container.Inject(hubMock);
            ObjectFactory.Container.Inject(hubMock.Object);
        }

        [Test]
        public async Task Configure_AfterInitialConfiguration_DataSourceSelectorContainsTableDataGenerators()
        {
            var activity = New<Monitor_Feed_Posts_v1>();
            
        }

      
    }
}
