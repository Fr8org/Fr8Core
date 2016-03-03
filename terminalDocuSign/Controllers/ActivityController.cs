using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AutoMapper;
using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.DataTransferObjects;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Controllers
{
    [RoutePrefix("activities")]
    public class ActivityController : BaseTerminalController
    {
        private const string curTerminal = "terminalDocuSign";
        [HttpPost]
        [fr8TerminalHMACAuthenticate(curTerminal)]
        [Authorize]
        public async Task<object> Execute([FromUri] String actionType, [FromBody] Fr8DataDTO curDataDTO)
        {


            try
            {
               var result = HandleFr8Request(curTerminal, actionType, curDataDTO);
               return await result.ContinueWith(x =>
                {
                    var res = result.Result;

                    ExternalLogger.Write("Response from '{0}' for {1}\nData:\n", curTerminal, actionType, JsonConvert.SerializeObject(res));

                    if (res == null)
                    {
                        return string.Format("Yes, we've just called '{0}' of type {1} and get null as the result", actionType, JsonConvert.ToString(curDataDTO));
                    }

                    return res;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("terminalDocuSign failed. {0}", ex.ToString());
                throw;
            }
        }
    }
}