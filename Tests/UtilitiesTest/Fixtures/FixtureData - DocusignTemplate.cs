using System;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static TemplateInfo TestDocuSignTemplateInfo1()
        {
            return new TemplateInfo
                   {
                       Name = "TestTemplate1",
                       Description = "this is a test"
            };
        }
    }
}