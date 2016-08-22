using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Models
{
    public struct ActivityRegistrationKey
    {
        public readonly ActivityTemplateDTO ActivityTemplateDTO;

        public ActivityRegistrationKey(ActivityTemplateDTO activityTemplateDTO)
        {
            ActivityTemplateDTO = activityTemplateDTO;
        }

        public ActivityRegistrationKey(string name, string version)
        {
            ActivityTemplateDTO = new ActivityTemplateDTO
            {
                Name = name,
                Version = version
            };
        }

        public bool Equals(ActivityRegistrationKey other)
        {
            return string.Equals(ActivityTemplateDTO.Name, other.ActivityTemplateDTO.Name) && string.Equals(ActivityTemplateDTO.Version, other.ActivityTemplateDTO.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ActivityRegistrationKey && Equals((ActivityRegistrationKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ActivityTemplateDTO.Name?.GetHashCode() ?? 0) * 397) ^ (ActivityTemplateDTO.Version?.GetHashCode() ?? 0);
            }
        }
    }
}
