using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using NUnit.Framework;
using StructureMap;
using terminalDropbox.Controllers;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test]
        public void TerminalDropbox_DiscoverTerminals_ShouldReturnDataInCorrectFormat()
        {
            //Arrange
            TerminalController _terminalController = ObjectFactory.GetInstance<TerminalController>();
            //Act
            var result = _terminalController.DiscoverTerminals();
            var actions = (result as JsonResult<StandardFr8TerminalCM>).Content.Actions;
            var terminal = (result as JsonResult<StandardFr8TerminalCM>).Content.Definition;
            //Assert
            Assert.IsNotNull(result, "The terminal discovery has failed for Terminal Dropbox");
            Assert.IsInstanceOf<WebServiceDTO>(actions.FirstOrDefault().WebService, "The WebService object is not of type WebServiceDTO");
            Assert.IsInstanceOf<TerminalDTO>(terminal, "The terminal object is not of type TerminalDTO");
            Assert.IsInstanceOf<ActivityTemplateDTO>(actions.FirstOrDefault(),
                "The action template object is not of type ActivityTemplateDTO");
        }

    }
}
