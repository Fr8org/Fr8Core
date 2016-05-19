using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Moq;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using terminalDocuSign.Activities;

namespace terminalDocuSignTests.Fixtures
{
    public static partial class DocuSignActivityFixtureData
    {
        public static BaseDocuSignActivity BaseDocuSignAcitvity()
        {
            var result = new Mock<BaseDocuSignActivity>();

            result.Setup(x => x.NeedsAuthentication())
                .Returns(false);

            result.Setup(x => x.NeedsAuthentication())
                .Returns(true);

            return result.Object;
        }

        public static BaseDocuSignActivity FailedBaseDocuSignActivity()
        {
            var result = new Mock<BaseDocuSignActivity>();
            /*result.Setup(x => x.Validate(It.IsAny<ActivityDO>(), It.IsAny<ICrateStorage>(), It.IsAny<ValidationManager>()))
                .Returns((ActivityDO x, ICrateStorage y, ValidationManager validationManager) =>
                {
                    validationManager.SetError("Error");
                    return Task.FromResult(0);
                });
                */
            return result.Object;
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
