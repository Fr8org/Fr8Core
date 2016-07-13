using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalAsana.Asana.Services
{
    public delegate void RefreshTokenEventHandler(object sender, AsanaRefreshTokenEventArgs e);

    public class AsanaRefreshTokenEventArgs
    {
        public AsanaRefreshTokenEventArgs(OAuthToken token)
        {
            RefreshedToken = token;
        }
        public OAuthToken RefreshedToken { get; set; }
    }
}