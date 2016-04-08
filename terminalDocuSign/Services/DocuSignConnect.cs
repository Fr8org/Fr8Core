using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Services
{
    public class DocuSignConnect : IDocuSignConnect
    {
        public string CreateConnect(DocuSignApiConfiguration conf, string name, string url)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            var connect = new ConnectConfiguration(name, url);
            var result = connectApi.CreateConnect(conf.AccountId, connect);
            return result.Data.connectId;
        }

        public List<ConnectConfiguration> ListConnects(DocuSignApiConfiguration conf)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            var connectInfo = connectApi.ListConnects(conf.AccountId);
            if (Convert.ToInt16(connectInfo.Data.totalRecords) > 0)
                return connectInfo.Data.configurations;
            else return new List<ConnectConfiguration>();
        }

        public string ActivateConnect(DocuSignApiConfiguration conf, ConnectConfiguration connect)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            connect.allowEnvelopePublish = "true";
            var response = connectApi.UpdateConnect(conf.AccountId, connect);
            return response.Data.connectId;
        }

        public string CreateOrActivateConnect(DocuSignApiConfiguration conf, string name, string url)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            var connects = ListConnects(conf);

            var existing_connect = connects.Where(a => a.name != null && a.name == name & a.urlToPublishTo != null).FirstOrDefault();
            if (existing_connect != null)
                return ActivateConnect(conf, existing_connect);
            else
                return CreateConnect(conf, name, url);
        }

        public void DeleteConnect(DocuSignApiConfiguration conf, string connectId)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            connectApi.DeleteConnect(conf.AccountId, connectId);
        }
    }
}