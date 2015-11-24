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

            _send_DocuSign_Envelope_v1 = new Send_DocuSign_Envelope_v1();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        [Test]
        public async Task UpdateUpstreamCrateTest()
        {
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();
            curActionDTO.AuthToken = new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) };
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            using (var updater = _crate.UpdateStorage(curActionDO))
            {
                var cnt = updater.CrateStorage.Count;
                var str = String.Format("{0}", cnt);

                var list = new List<Crate<StandardDesignTimeFieldsCM>>();
                var crate = new Crate<StandardDesignTimeFieldsCM>(new Crate(CrateManifestType.Unknown, "testlabel"));
                list.Add(crate);
                
                Crate<StandardDesignTimeFieldsCM>[] curCrates = new Crate<StandardDesignTimeFieldsCM>[1];
                curCrates.SetValue(crate, 0);//.AddRange<StandardDesignTimeFieldsCM>(list);

                //var routeNode = MockGetCrates<StandardDesignTimeFieldsCM>(curCrates);
                var mock = MockGetCrates<StandardDesignTimeFieldsCM>(curCrates);

                IRouteNode routeNode = mock.Object;

                var result = routeNode.GetCratesByDirection<StandardDesignTimeFieldsCM>(curActionDO.Id, CrateDirection.Upstream);

                str = String.Format(result.ToString());
                _send_DocuSign_Envelope_v1.UpdateUpstreamCrate(curActionDO);

            
                // Build a crate with the list of available upstream fields
                cnt = updater.CrateStorage.Count;
                str = String.Format("{0}", cnt);
                var item = updater.CrateStorage.FirstOrDefault<Crate>();
                str = String.Format("{0}", item.ToString());
                var has = updater.CrateStorage.Any(c => c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                                                                                    && c.Label == "Upstream Terminal-Provided Fields");
                Assert.IsTrue(has);
            }

        }

        private Mock<IRouteNode> MockGetCrates<TManifest>(params Crate<TManifest>[] cratesToReturn)
        {
            var mok = new Mock<IRouteNode>();

            mok.Setup(x => x.GetCratesByDirection<TManifest>(It.IsAny<Guid>(), It.IsAny<CrateDirection>())).Returns(Task.FromResult(new List<Crate<TManifest>>(cratesToReturn)));

            return mok;
        }
    }

}