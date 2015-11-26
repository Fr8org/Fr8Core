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

            var listTestCrates = FixtureData.TestCrateDTO1();
            Crate<StandardDesignTimeFieldsCM> crate = new Crate<StandardDesignTimeFieldsCM>(listTestCrates[0]);
            Crate<StandardDesignTimeFieldsCM>[] curCrates = new Crate<StandardDesignTimeFieldsCM>[1];
            curCrates.SetValue(crate, 0);

            using (var updater = _crate.UpdateStorage(curActionDO))
            {  
                // Build a crate with the list of available upstream fields
                var curUpstreamFieldsCrate = updater.CrateStorage.SingleOrDefault(c =>
                                                                                    c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                && c.Label == "Upstream Terminal-Provided Fields");

                if (curUpstreamFieldsCrate != null)
                {
                    updater.CrateStorage.Remove(curUpstreamFieldsCrate);
                }

                var curUpstreamFields = GetDesignTimeFields(curActionDO, curCrates, CrateDirection.Upstream)
                    .Fields
                    .ToArray();

                curUpstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);
                updater.CrateStorage.Add(curUpstreamFieldsCrate);
                
               
                var has = updater.CrateStorage.Any(c => c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                                                                                    && c.Label == "Upstream Terminal-Provided Fields");
                Assert.IsTrue(has);
            }

        }

        private StandardDesignTimeFieldsCM GetDesignTimeFields(ActionDO actionDO, Crate<StandardDesignTimeFieldsCM>[] curCrates, CrateDirection direction)
        {
            

            var mock = MockGetCrates<StandardDesignTimeFieldsCM>(curCrates);
            IRouteNode routeNode = mock.Object;

            Task<List <Crate<StandardDesignTimeFieldsCM>>> lisrCrates =  (routeNode.GetCratesByDirection<StandardDesignTimeFieldsCM>(actionDO.Id, CrateDirection.Upstream));

            //1) Build a merged list of the upstream design fields to go into our drop down list boxes
            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();

            mergedFields.Fields.AddRange(_send_DocuSign_Envelope_v1.MergeContentFields(lisrCrates.Result).Fields);

            return mergedFields;
        }

        private Mock<IRouteNode> MockGetCrates<TManifest>(params Crate<TManifest>[] cratesToReturn)
        {
            var mok = new Mock<IRouteNode>();

            mok.Setup(x => x.GetCratesByDirection<TManifest>(It.IsAny<Guid>(), It.IsAny<CrateDirection>())).Returns(Task.FromResult(new List<Crate<TManifest>>(cratesToReturn)));

            return mok;
        }
    }

}