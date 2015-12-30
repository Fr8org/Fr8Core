
using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public FileDO TestFile1()
        {
            FileDO file = new FileDO
            {
                CloudStorageUrl = string.Empty
            };

            return file;
        }
    }
}
