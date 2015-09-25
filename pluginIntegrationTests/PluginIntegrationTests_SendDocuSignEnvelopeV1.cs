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

namespace pluginIntegrationTests
{
	public partial class PluginIntegrationTests : BaseTest
	{
		/// <summary>
		/// Test Send_DocuSign_Envelope_v1 initial configuration.
		/// </summary>
		[Test]
		public void PluginIntegration_SendDocuSignEnvelopeV1_ConfigureInitial()
		{
			var savedActionDTO = CreateEmptyAction();
			WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
		}

		/// <summary>
		/// Test Send_DocuSign_Envelope_v1 follow-up configuration.
		/// </summary>
		[Test]
		public void PluginIntegration_SendDocuSignEnvelopeV1_ConfigureFollowUp()
		{

		}
	}
}
