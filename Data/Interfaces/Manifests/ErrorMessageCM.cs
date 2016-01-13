using Data.Constants;

namespace Data.Interfaces.Manifests
{
    public class ErrorMessageCM : Manifest
    {
        public ErrorMessageCM()
            : base(Constants.MT.ErrorMessage)
        {
        }

        public string Message { get; set; }
        public string CurrentAction { get; set; }
    }
}
