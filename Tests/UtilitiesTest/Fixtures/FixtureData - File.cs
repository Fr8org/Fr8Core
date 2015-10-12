
using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public FileDO TestFile1()
        {
            FileDO file = new FileDO
            {
                DocuSignEnvelopeID = 1,
                DocuSignTemplateID = 1,
                CloudStorageUrl = string.Empty
            };

            return file;
        }
    }
}
