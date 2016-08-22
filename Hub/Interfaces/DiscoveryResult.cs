using System.Collections.Generic;
using Data.Entities;

namespace Hub.Interfaces
{
    public class DiscoveryResult
    {
        public bool IsSucceed { get; set; }
        public string ErrorMessage { get; set; }
        public readonly List<ActivityTemplateDO> FailedTemplates  = new List<ActivityTemplateDO>();
        public readonly List<ActivityTemplateDO> SucceededTemplates = new List<ActivityTemplateDO>();

        public DiscoveryResult(bool isSucceed, string errorMessage)
        {
            IsSucceed = isSucceed;
            ErrorMessage = errorMessage;
        }

        public static DiscoveryResult Error(string message)
        {
            return new DiscoveryResult(false, message);
        }

        public static DiscoveryResult Ok()
        {
            return new DiscoveryResult(true, null);
        }
    }
}