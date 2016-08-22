using System;
using System.Threading.Tasks;
using Hub.Enums;

namespace Hub.Interfaces
{
    public interface IManifestPageGenerator
    {
        Task<Uri> Generate(string manifestName, GenerateMode generateMode);
    }
}
