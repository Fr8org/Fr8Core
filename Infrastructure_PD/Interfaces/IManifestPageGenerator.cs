using System;
using System.Threading.Tasks;

namespace HubWeb.Infrastructure_PD.Interfaces
{
    public interface IManifestPageGenerator
    {
        Task<Uri> Generate(string manifestName, GenerateMode generateMode);
    }
}
