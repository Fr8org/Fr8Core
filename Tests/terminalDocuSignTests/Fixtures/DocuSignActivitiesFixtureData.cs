using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Moq;
using Moq.Protected;
using terminalDocuSign;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using terminalDocuSign.Activities;

namespace terminalDocuSignTests.Fixtures
{
    public class BaseDocusignActivityMock : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "BaseDocusignActivityMock",
            Label = "BaseDocusignActivityMock",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        protected override string ActivityUserFriendlyName { get; }
        protected override Task RunDS()
        {
            return Task.FromResult(0);
        }

        protected override Task InitializeDS()
        {
            return Task.FromResult(0);
        }

        protected override Task FollowUpDS()
        {
            return Task.FromResult(0);
        }
    }

    public static partial class DocuSignActivityFixtureData
    {
        public static BaseDocusignActivityMock BaseDocuSignAcitvity()
        {
            return new BaseDocusignActivityMock();
            /*
            var result = new Mock<BaseDocusignActivityMock>();

            result.Setup(x => x.NeedsAuthentication())
                .Returns(false);

            result.Setup(x => x.NeedsAuthentication())
                .Returns(true);

            return result.Object;*/
        }

        public static BaseDocusignActivityMock FailedBaseDocuSignActivity()
        {
            /*
            var result = new Mock<BaseDocusignActivityMock>();
            result.Setup(x => x.Validate())
                .Returns(() =>
                {
                    ValidationManager.SetError("Error");
                    return Task.FromResult(0);
                });

            result.Protected().Setup<Task>("Validate").Returns(() =>
            {
                result.Object.ValidationManager.SetError("Error");
                return Task.FromResult(0);
            });

            return result.Object;*/
            return new BaseDocusignActivityMock();
        }

        public static IDocuSignManager DocuSignManagerWithTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()))
                  .Returns(new List<FieldDTO> { new FieldDTO("1", "First") });
            return result.Object;
        }

        public static IDocuSignManager DocuSignManagerWithoutTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()))
                  .Returns(new List<FieldDTO>());
            return result.Object;
        }
    }
}
