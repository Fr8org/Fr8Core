using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalAzure.Controllers;
using Utilities;
using UtilitiesTesting.Fixtures;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test]
        public void TerminalAzure_DiscoverTerminals_ShouldReturnDataInCorrectFormat()
        {
            //Arrange
            TerminalController _terminalController = ObjectFactory.GetInstance<TerminalController>();

            //Act
            var result = _terminalController.DiscoverTerminals();
            var actions = (result as JsonResult<StandardFr8TerminalCM>).Content.Actions;
            var terminal = (result as JsonResult<StandardFr8TerminalCM>).Content.Definition;
            //Assert
            Assert.IsNotNull(result, "The terminal discovery has failed for Terminal Azure");
            Assert.IsInstanceOf<WebServiceDTO>(actions.FirstOrDefault().WebService, "The WebService object is not of type WebServiceDTO");
            Assert.IsInstanceOf<TerminalDTO>(terminal, "The terminal object is not of type TerminalDTO");
            Assert.IsInstanceOf<ActivityTemplateDTO>(actions.FirstOrDefault(),
                "The action template object is not of type ActivityTemplateDTO");
        }

    }
}

