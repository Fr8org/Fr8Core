using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
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
