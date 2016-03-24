using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json.Linq;
using terminalDocuSign.DataTransferObjects;
using DocuSign.eSign.Model;

namespace terminalDocuSign.Services.New_Api
{
    public interface IDocuSignConnect
    {
        string CreateConnect(DocuSignApiConfiguration conf, string name, string url);

        List<ConnectConfiguration> ListConnects(DocuSignApiConfiguration conf);

        void ActivateConnect(DocuSignApiConfiguration conf, ConnectConfiguration connect);

        void CreateOrActivateConnect(DocuSignApiConfiguration conf, string name, string url);
    }
}