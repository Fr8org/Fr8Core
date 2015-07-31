using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static EnvelopeDO CreateEnvelope()
		{
			return new EnvelopeDO { DocusignEnvelopeId = "21", Status = EnvelopeDO.EnvelopeState.Any};
		}
	}
}