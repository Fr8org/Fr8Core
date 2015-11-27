using AutoMapper;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.Infrastructure;
using terminalDocuSign.Actions;
using terminalDocuSign.Tests.Fixtures;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("terminalDocuSign")]
    public class Send_DocuSign_Envelope_v1Tests : BaseTest
    {
        Send_DocuSign_Envelope_v1 _send_DocuSign_Envelope_v1;

        ICrateManager _crate;

        public Send_DocuSign_Envelope_v1Tests()
        {
            base.SetUp();

            TerminalBootstrapper.ConfigureTest();            

            _send_DocuSign_Envelope_v1 = new Send_DocuSign_Envelope_v1();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        [Test]
        public async Task UpdateUpstreamCrateTest()
        {
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();
            curActionDTO.AuthToken = new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) };
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);            

            var crate = FixtureData.TestCrateDTO1();
            Crate<StandardDesignTimeFieldsCM>[] curCrates = new Crate<StandardDesignTimeFieldsCM>[1];
            curCrates.SetValue(crate[0], 0);
     
            MockGetCrates<StandardDesignTimeFieldsCM>(curCrates);

            _send_DocuSign_Envelope_v1.UpdateUpstreamCrate(curActionDO);

            using (var updater = _crate.UpdateStorage(curActionDO))
            { 
                var has = updater.CrateStorage.Any(c => c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                                                                                    && c.Label == "Upstream Terminal-Provided Fields");

                Assert.IsTrue(has);
            }

        }
        
        private void MockGetCrates<TManifest>(params Crate<TManifest>[] cratesToReturn)
        {
            var communicator = _send_DocuSign_Envelope_v1.HubCommunicator;
            var mok = Mock.Get(communicator);

            mok.Setup(x => x.GetCratesByDirection<TManifest>(It.IsAny<ActionDO>(), It.IsAny<CrateDirection>())).Returns(Task.FromResult(new List<Crate<TManifest>>(cratesToReturn)));
            
        }
    }

}