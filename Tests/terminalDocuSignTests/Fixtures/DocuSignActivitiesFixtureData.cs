using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Moq;
using terminalDocuSign;
using terminalDocuSign.Services.New_Api;
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
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        protected override string ActivityUserFriendlyName { get; }


        public BaseDocusignActivityMock(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }

    public static partial class DocuSignActivityFixtureData
    {
       
        public static IDocuSignManager DocuSignManagerWithTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()))
                  .Returns(new List<KeyValueDTO> { new KeyValueDTO("1", "First") });
            return result.Object;
        }

        public static IDocuSignManager DocuSignManagerWithoutTemplates()
        {
            var result = new Mock<IDocuSignManager>();
            result.Setup(x => x.GetTemplatesList(It.IsAny<DocuSignApiConfiguration>()))
                  .Returns(new List<KeyValueDTO>());
            return result.Object;
        }
    }
}
