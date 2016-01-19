﻿using System;
using System.Configuration;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using Hub.Infrastructure;
using StructureMap;
using Hub.Interfaces;
using Utilities.Configuration.Azure;

namespace TerminalBase.Infrastructure
{
    public class fr8TerminalHMACAuthorizeAttribute : fr8HMACAuthorizeAttribute
    {
        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }
        public fr8TerminalHMACAuthorizeAttribute(string terminalName)
        {
            TerminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting("TerminalId");

            //we might be on integration test currently
            if (TerminalSecret == null || TerminalId == null)
            {
                TerminalSecret = ConfigurationManager.AppSettings[terminalName + "TerminalSecret"];
                TerminalId = ConfigurationManager.AppSettings[terminalName + "TerminalId"];
            }
        }

        protected override async Task<string> GetTerminalSecret(string terminalId)
        {
            if (terminalId == TerminalId)
            {
                return TerminalSecret;
            }

            return null;
        }

        protected override Task<bool> CheckPermission(string terminalId, string userId)
        {
            return Task.FromResult(true);
        }
    }
}