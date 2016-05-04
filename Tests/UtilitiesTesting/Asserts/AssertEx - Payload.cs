using System.Linq;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;

namespace UtilitiesTesting.Asserts
{
    public static partial class AssertEx
    {
        public static void AssertPayloadHasAuthenticationError(PayloadDTO payload)
        {
            var storage = new CrateManager().GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            ErrorDTO errorMessage;
            operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);
            Assert.AreEqual(ActivityResponse.Error.ToString(), operationalStateCM.CurrentActivityResponse.Type);
            Assert.AreEqual(ActivityErrorCode.AUTH_TOKEN_NOT_PROVIDED_OR_INVALID, operationalStateCM.CurrentActivityErrorCode);
            Assert.AreEqual("No AuthToken provided.", errorMessage.Message);
        }

        public static void AssertPayloadHasError(PayloadDTO payload)
        {
            var storage = new CrateManager().GetStorage(payload);
            var operationalStateCM = storage.CrateContentsOfType<OperationalStateCM>().Single();
            ErrorDTO errorMessage;
            Assert.IsTrue(operationalStateCM.CurrentActivityResponse.TryParseErrorDTO(out errorMessage));
        }
    }
}
