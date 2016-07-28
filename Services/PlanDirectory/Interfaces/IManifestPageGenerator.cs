using System;
using System.Threading.Tasks;

namespace PlanDirectory.Interfaces
{
    public interface IManifestPageGenerator
    {
        Task<Uri> Generate(string manifestName, GenerateMode generateMode);
    }
}
