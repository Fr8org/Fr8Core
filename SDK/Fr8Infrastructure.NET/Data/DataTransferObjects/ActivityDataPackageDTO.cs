
namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class ActivityDataPackageDTO
    {
        public ActivityDataPackageDTO(ActivityDTO curActionDTO, PayloadDTO curPayloadDTO)
        {
            ActivityDTO = curActionDTO;
            PayloadDTO = curPayloadDTO;
        }
        public ActivityDTO ActivityDTO { get; set; }
        public PayloadDTO PayloadDTO { get; set; }
    }
}
