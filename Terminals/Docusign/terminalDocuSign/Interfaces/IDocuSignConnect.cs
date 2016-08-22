using System.Collections.Generic;
using DocuSign.eSign.Model;

namespace terminalDocuSign.Services.New_Api
{
    public interface IDocuSignConnect
    {
        string CreateConnect(DocuSignApiConfiguration conf, string name, string url);

        List<ConnectConfiguration> ListConnects(DocuSignApiConfiguration conf);

        string ActivateConnect(DocuSignApiConfiguration conf, ConnectConfiguration connect);

        string CreateOrActivateConnect(DocuSignApiConfiguration conf, string name, string url);

        void DeleteConnect(DocuSignApiConfiguration conf, string connectId);
    }
}