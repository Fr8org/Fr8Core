using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoMapper;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Controllers;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalAzure;

namespace terminalIntegrationTests
{
	public partial class TerminalIntegrationTests : BaseTest
	{
	    

		/// <summary>
		/// Test Send_DocuSign_Envelope_v1 initial configuration.
		/// </summary>
		[Test, Ignore]
        public async Task TerminalIntegration_SendDocuSignEnvelope_ConfigureInitial()
		{
			var curActionDTO = CreateEmptyActivity(_sendDocuSignEnvelopeActivityTemplate);
			await SendDocuSignEnvelope_ConfigureInitial(curActionDTO);
		}

	    /// <summary>
	    /// Test Send_DocuSign_Envelope_v1 follow-up configuration.
	    /// </summary>
	    [Test, Ignore]
        public async Task TerminalIntegration_SendDocuSignEnvelopeV1_ConfigureFollowUp()
		{
			// Create blank WaitForDocuSignEventAction.
			var savedActionDTO = CreateEmptyActivity(_sendDocuSignEnvelopeActivityTemplate);

			// Call Configure Initial for WaitForDocuSignEvent action.
			var initCrateStorageDTO = await SendDocuSignEnvelope_ConfigureInitial(savedActionDTO);

			// Select first available DocuSign template.
			SendDocuSignEnvelope_SelectFirstTemplate(initCrateStorageDTO);

		    using (var crateStorage = _crateManager.GetUpdatableStorage(savedActionDTO))
		    {
                crateStorage.Replace(initCrateStorageDTO);
		    }

		   // FixActionNavProps(savedActionDTO.Id);

			// Call Configure FollowUp for SendDocuSignEnvelope action.
			await SendDocuSignEnvelope_ConfigureFollowUp(savedActionDTO);

		}
		private async Task<ICrateStorage> SendDocuSignEnvelope_ConfigureInitial(ActivityDTO curActionDTO)
		{
			// Fill values as it would be on front-end.
		    curActionDTO.CrateStorage = new CrateStorageDTO();

			// Send initial configure request.
			var curActionController = CreateActivityController();
			var activityDTO = await curActionController.Configure(curActionDTO)
				 as OkNegotiatedContentResult<ActivityDTO>;

			// Assert initial configuration returned in CrateStorage.
			Assert.NotNull(activityDTO);
			Assert.NotNull(activityDTO.Content);
			Assert.NotNull(activityDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(activityDTO.Content);

            Assert.AreEqual(storage.Count, 3);
            Assert.True((storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls")));
            Assert.True((storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "Available Templates")));

			return storage;
		}
		
        private void SendDocuSignEnvelope_SelectFirstTemplate(ICrateStorage curCrateStorage)
		{
			// Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrateDTO = curCrateStorage.CratesOfType<FieldDescriptionsCM>().Single(x => x.Label == "Available Templates");

		    var fieldsMS = availableTemplatesCrateDTO.Content;

			// Fetch Configuration Controls crate and parse StandardConfigurationControlsMS

            var configurationControlsCrateDTO = curCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single(x => x.Label == "Configuration_Controls");

            var controlsMS = configurationControlsCrateDTO.Content;

			// Modify value of Selected_DocuSign_Template field and push it back to crate,
			// exact same way we do on front-end.
			var docuSignTemplateControlDTO = controlsMS.Controls.Single(x => x.Name == "target_docusign_template");
			docuSignTemplateControlDTO.Value = fieldsMS.Fields.First().Value;
		}

		private async Task<ICrateStorage> SendDocuSignEnvelope_ConfigureFollowUp(ActivityDTO curActionDTO)
		{
			var curActionController = CreateActivityController();

			var activityDTO = await curActionController.Configure(curActionDTO)
				 as OkNegotiatedContentResult<ActivityDTO>;

			// Assert FollowUp Configure result.
			Assert.NotNull(activityDTO);
			Assert.NotNull(activityDTO.Content);
			Assert.NotNull(activityDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(activityDTO.Content);

            Assert.AreEqual(storage.Count, 2);
            Assert.True((storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "DocuSignTemplateUserDefinedFields")));
            Assert.True((storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "DocuSignTemplateStandardFields")));

			return storage;
		}
	}
}
