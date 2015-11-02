using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using HubWeb.Controllers;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class ReportControllerTest : BaseTest
    {
        private ReportController _reportController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _reportController = new ReportController();
        }
     
    }
}
