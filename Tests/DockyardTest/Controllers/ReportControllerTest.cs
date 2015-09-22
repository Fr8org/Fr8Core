using System.Linq;
using System.Web.Http.Results;
using Core.Managers;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Data.Entities;
using System.Collections.Generic;
using Utilities;
using Newtonsoft.Json;


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
