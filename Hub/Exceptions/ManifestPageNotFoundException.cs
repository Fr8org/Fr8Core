namespace Hub.Exceptions
{
    public class ManifestPageNotFoundException : ManifestGenerationException
    {
        public ManifestPageNotFoundException(string manifestName) : base($"Page definition for manifest '{manifestName}' doesn't exist")
        {
        }
    }
}