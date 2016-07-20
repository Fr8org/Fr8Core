using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.Helpers;

namespace HubTests.Fixtures
{
    public partial class FixtureData
    {
        public const string ErrorMessage = "ErrorMessage";
        public static ActivityResponseDTO ErrorActivityResponseDTOWithErrorMessage()
        {
            var result = new ActivityResponseDTO { Type = ActivityResponse.Error.ToString() };
            result.AddErrorDTO(ErrorDTO.InternalError(ErrorMessage));
            return result;
        }

        public static ActivityResponseDTO ErrorActivityResponseDTOWithoutErrorMessage()
        {
            var result = new ActivityResponseDTO();
            result.Type = ActivityResponse.Error.ToString();
            return result;
        }
    }
}
