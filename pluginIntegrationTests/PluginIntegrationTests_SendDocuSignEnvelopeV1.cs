using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using AutoMapper;
using Core.Interfaces;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using pluginAzureSqlServer;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using UtilitiesTesting;
using Data.Constants;
using Utilities;
using System.Threading.Tasks;

namespace pluginIntegrationTests
{
	public partial class PluginIntegrationTests : BaseTest
	{
		/// <summary>
		/// Test Send_DocuSign_Envelope_v1 initial configuration.
		/// </summary>
		[Test, Ignore]
		public async Task PluginIntegration_SendDocuSignEnvelope_ConfigureInitial()
		{
			var curActionDTO = CreateEmptyAction(_sendDocuSignEnvelopeActivityTemplate);
			await SendDocuSignEnvelope_ConfigureInitial(curActionDTO);
		}
		/// <summary>
		/// Test Send_DocuSign_Envelope_v1 follow-up configuration.
		/// </summary>
		[Test, Ignore]
		public async Task PluginIntegration_SendDocuSignEnvelopeV1_ConfigureFollowUp()
		{
			// Create blank WaitForDocuSignEventAction.
			var savedActionDTO = CreateEmptyAction(_sendDocuSignEnvelopeActivityTemplate);

			// Call Configure Initial for WaitForDocuSignEvent action.
			var initCrateStorageDTO = await SendDocuSignEnvelope_ConfigureInitial(savedActionDTO);

			// Select first available DocuSign template.
			SendDocuSignEnvelope_SelectFirstTemplate(initCrateStorageDTO);
			savedActionDTO.CrateStorage = initCrateStorageDTO;

            FixActionNavProps(savedActionDTO.Id);

			// Call Configure FollowUp for SendDocuSignEnvelope action.
			await SendDocuSignEnvelope_ConfigureFollowUp(savedActionDTO);

		}
		private async Task<CrateStorageDTO> SendDocuSignEnvelope_ConfigureInitial(ActionDTO curActionDTO)
		{
			// Fill values as it would be on front-end.
			curActionDTO.ActivityTemplateId = _sendDocuSignEnvelopeActivityTemplate.Id;
			curActionDTO.CrateStorage = new CrateStorageDTO();

			// Send initial configure request.
			var curActionController = CreateActionController();
			var actionDTO = await curActionController.Configure(curActionDTO)
				 as OkNegotiatedContentResult<ActionDTO>;

			// Assert initial configuration returned in CrateStorage.
			Assert.NotNull(actionDTO);
			Assert.NotNull(actionDTO.Content);
			Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
			Assert.AreEqual(actionDTO.Content.CrateStorage.CrateDTO.Count, 3);
			Assert.True((actionDTO.Content.CrateStorage.CrateDTO
				 .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == MT.StandardConfigurationControls.GetEnumDisplayName())));
			Assert.True(actionDTO.Content.CrateStorage.CrateDTO
				 .Any(x => x.Label == "Available Templates" && x.ManifestType == MT.StandardDesignTimeFields.GetEnumDisplayName()));

			return actionDTO.Content.CrateStorage;
		}
		private void SendDocuSignEnvelope_SelectFirstTemplate(CrateStorageDTO curCrateStorage)
		{
			// Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
			var availableTemplatesCrateDTO = curCrateStorage.CrateDTO
				 .Single(x => x.Label == "Available Templates" && x.ManifestType == MT.StandardDesignTimeFields.GetEnumDisplayName());

			var fieldsMS = JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(
				 availableTemplatesCrateDTO.Contents);

			// Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
			var configurationControlsCrateDTO = curCrateStorage.CrateDTO
				 .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == MT.StandardConfigurationControls.GetEnumDisplayName());

			var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
				 configurationControlsCrateDTO.Contents);

			// Modify value of Selected_DocuSign_Template field and push it back to crate,
			// exact same way we do on front-end.
			var docuSignTemplateControlDTO = controlsMS.Controls.Single(x => x.Name == "target_docusign_template");
			docuSignTemplateControlDTO.Value = fieldsMS.Fields.First().Value;

			configurationControlsCrateDTO.Contents = JsonConvert.SerializeObject(controlsMS);
		}
		private async Task<CrateStorageDTO> SendDocuSignEnvelope_ConfigureFollowUp(ActionDTO curActionDTO)
		{
			var curActionController = CreateActionController();

			var actionDTO = await curActionController.Configure(curActionDTO)
				 as OkNegotiatedContentResult<ActionDTO>;

			// Assert FollowUp Configure result.
			Assert.NotNull(actionDTO);
			Assert.NotNull(actionDTO.Content);
			Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
			Assert.AreEqual(2, actionDTO.Content.CrateStorage.CrateDTO.Count);
			Assert.True(actionDTO.Content.CrateStorage.CrateDTO
				 .Any(x => x.Label == "DocuSignTemplateUserDefinedFields" && x.ManifestType == MT.StandardDesignTimeFields.GetEnumDisplayName()));
			Assert.True(actionDTO.Content.CrateStorage.CrateDTO
				 .Any(x => x.Label == "DocuSignTemplateStandardFields" && x.ManifestType == MT.StandardDesignTimeFields.GetEnumDisplayName()));
			return actionDTO.Content.CrateStorage;
		}
	}
}
