namespace Hub.Exceptions
{
    public class ManifestNotFoundException : ManifestGenerationException
    {
        public ManifestNotFoundException(string manifestName) : base($"Manifest '{manifestName}' was not found")
        {
        }
    }
}