using Core.Models.DocuSign;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static DocuSignEnvelopeInformation TestDocuSignEnvelopeInformation()
		{
			var info = new DocuSignEnvelopeInformation();
			info.EnvelopeStatus = new EnvelopeStatus();
			info.EnvelopeStatus.EnvelopeId = "0aa561b8-b4d9-47e0-a615-2367971f876b";
			info.EnvelopeStatus.RecipientStatuses = new RecipientStatuses
			{
				Statuses = new[]
				{
					new RecipientStatus { Id = "fb89d2ee-2876-4290-b530-ff1833d5d0d2" }
				}
			};
			return info;
		}
	}
}