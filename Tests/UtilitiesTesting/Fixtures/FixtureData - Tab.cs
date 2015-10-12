using System.Collections.Generic;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static TabCollection TestTabCollection1()
        {
            return new TabCollection
                   {
                       textTabs = new List<TextTab>
                                  {
                                      TestTab1()
                                  }
                   };
        }

        public static TextTab TestTab1()
        {
            return new TextTab
                   {
                       required = false,
                       height = 200,
                       width = 200,
                       xPosition = 200,
                       yPosition = 200,
                       name = "Amount",
                       value = "40"
                   };
        }
    }
}
