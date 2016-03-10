using System.Collections.Generic;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Moq;
using terminalDocuSign.Actions;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;

namespace terminalDocuSignTests.Fixtures
{
    public static partial class DocuSignActivityFixtureData
    {
        public static BaseDocuSignActivity BaseDocuSignAcitvity()
        {
            return new Mock<BaseDocuSignActivity>().Object;
        }

        public static BaseDocuSignActivity FailedBaseDocuSignActivity()
        {
            var result = new Mock<BaseDocuSignActivity>();
            string errorMessage;
            result.Setup(x => x.ActivityIsValid(It.IsAny<ActivityDO>(), out errorMessage))
                  .Returns(false);
            return result.Object;
        }

        public static IDocuSignManager DocuSignManagerWithTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.FillDocuSignTemplateSource(It.IsAny<Crate>(), It.IsAny<string>(), It.IsAny<DocuSignAuthTokenDTO>()))
                  .Callback<Crate, string, DocuSignAuthTokenDTO>((crate, name, token) =>
                            {
                                var configurationControl = crate.Get<StandardConfigurationControlsCM>();
                                var control = configurationControl.FindByNameNested<DropDownList>(name);
                                if (control != null)
                                {
                                    control.ListItems = new List<ListItem> { new ListItem { Key = "1", Value = "First" } };
                                }
                            });
            return result.Object;
        }

        public static IDocuSignManager DocuSignManagerWithoutTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.FillDocuSignTemplateSource(It.IsAny<Crate>(), It.IsAny<string>(), It.IsAny<DocuSignAuthTokenDTO>()))
                  .Callback<Crate, string, DocuSignAuthTokenDTO>((crate, name, token) =>
                  {
                      var configurationControl = crate.Get<StandardConfigurationControlsCM>();
                      var control = configurationControl.FindByNameNested<DropDownList>(name);
                      if (control != null)
                      {
                          control.ListItems = new List<ListItem>();
                      }
                  });
            return result.Object;
        }
    }
}
