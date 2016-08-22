using System.Linq;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;
using Fr8.Infrastructure.Data.Manifests;
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
            Assert.AreEqual(ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID.ToString(), errorMessage.ErrorCode);
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
