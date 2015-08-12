using System.Collections.Generic;
using System.Configuration;
using Core.Interfaces;
using Newtonsoft.Json;
using Data.Entities;

namespace Core.PluginRegistrations
{
    public class AzureSqlPluginRegistration : BasePluginRegistration
    {
        public const string BaseUrlKey = "AzureSql.BaseUrl";
        private const string _AvailableActions = @"[{ ""ActionType"" : ""Write To Sql Server"" , ""Version"": ""1.3""},
                                                    {""ActionType"" : ""Write To Sql Server"", ""Version"" : ""1.4""},
                                                    {""ActionType"" : ""Read From Sql Server"", ""Version"" : ""1.6""}]";

        private readonly IAction _action;

        public AzureSqlPluginRegistration(IAction action)
            : base(action, _AvailableActions, BaseUrlKey)
        {
            this._action = action;
        }
    }
}
