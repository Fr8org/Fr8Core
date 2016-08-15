using System;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class Fr8DataDTO
    {
        public ActivityDTO ActivityDTO { get; set; }
        public Guid? ContainerId { get; set; }
        
        /// <summary>
        /// This property is used for integration tests
        /// </summary>
        public string ExplicitData { get; set; }
    }
}
