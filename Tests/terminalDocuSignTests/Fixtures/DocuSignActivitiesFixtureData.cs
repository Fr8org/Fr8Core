using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Migrations;
using Moq;
using terminalDocuSign.Actions;
using UtilitiesTesting.Fixtures;

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
    }
}
