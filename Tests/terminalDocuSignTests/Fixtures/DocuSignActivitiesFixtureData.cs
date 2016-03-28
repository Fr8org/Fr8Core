using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Moq;
using terminalDocuSign.Actions;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSignTests.Fixtures
{
    public static partial class DocuSignActivityFixtureData
    {
        public static BaseDocuSignActivity BaseDocuSignAcitvity()
        {
            var result = new Mock<BaseDocuSignActivity>();

            result.Setup(x => x.NeedsAuthentication(It.IsNotNull<AuthorizationTokenDO>()))
                .Returns(false);

            result.Setup(x => x.NeedsAuthentication(null))
                .Returns(true);

            return result.Object;
        }

        public static BaseDocuSignActivity FailedBaseDocuSignActivity()
        {
            var result = new Mock<BaseDocuSignActivity>();
            result.Setup(x => x.ValidateActivityInternal(It.IsAny<ActivityDO>()))
                  .Returns(new ValidationResult("Failed"));
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
