using Data.Wrappers;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static DocuSignAccount TestDocuSignAccount1()
        {
            // credentials for sending account
            return new DocuSignAccount
                   {
                       Email = "DocuSignTestAccount1@Dockyard.company",
                       Password = "peach23",
                       UserId = " DocuSignTestAccount1@Dockyard.company",
                       UserName = " DocuSignTestAccount1@Dockyard.company"
            };
        }
    }
}