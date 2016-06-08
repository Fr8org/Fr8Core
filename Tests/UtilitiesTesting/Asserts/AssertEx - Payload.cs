using System.Linq;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.Crates;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;

namespace UtilitiesTesting.Asserts
{
    public static partial class AssertEx
    {
        public static void AssertPayloadHasAuthenticationError(ICrateStorage storage)
        {
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual(ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID, operationalStateCM.CurrentActivityErrorCode);
            Assert.AreEqual("No AuthToken provided.", errorMessage.Message);
        }

        public static void AssertPayloadHasError(ICrateStorage storage)
        {
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            ErrorDTO errorMessage;
            Assert.IsTrue(operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage));
        }
    }
}
