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

        public void ActivateConnect(DocuSignApiConfiguration conf, ConnectConfiguration connect)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            connect.allowEnvelopePublish = "true";
            connectApi.UpdateConnect(conf.AccountId, connect);
        }

        public void CreateOrActivateConnect(DocuSignApiConfiguration conf, string name, string url)
        {
            var connectApi = new ConnectApi(conf.Configuration);
            var connects = ListConnects(conf);

            var existing_connect = connects.Where(a => a.name != null && a.name == name & a.urlToPublishTo != null && a.urlToPublishTo == url).FirstOrDefault();
            if (existing_connect != null)
                ActivateConnect(conf, existing_connect);
            else
                CreateConnect(conf, name, url);
        }
    }
}