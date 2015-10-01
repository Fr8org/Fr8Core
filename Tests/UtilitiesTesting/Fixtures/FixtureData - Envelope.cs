using System;
using Data.Entities;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static EnvelopeDO TestEnvelope1()
		{
			return new EnvelopeDO
			{
				DocusignEnvelopeId = "21",
				EnvelopeStatus = EnvelopeDO.EnvelopeState.Any
			};
		}

		

	    public static string TestTemplateId = "58521204-58AF-4E65-8A77-4F4B51FEF626";
	}
}