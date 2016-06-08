using System.Linq;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.DataTransferObjects.Helpers;
using Fr8Data.Manifests;
using NUnit.Framework;

namespace Fr8.Testing.Unit.Asserts
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
