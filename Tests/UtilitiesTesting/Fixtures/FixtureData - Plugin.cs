using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static PluginDO PluginOne()
        {
            return new PluginDO
            {
                Name = "pluginAzureSqlServer",
                Endpoint = "pluginAzureSqlServer",
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static PluginDO PluginTwo()
        {
            return new PluginDO
            {
                Name = "AzureSqlServer",
                Endpoint = "AzureSqlServer",
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static PluginDO PluginThree()
        {
            return new PluginDO
            {
                Name = "http://localhost:46281/",
                Endpoint = "http://localhost:46281/",
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }
        public static PluginDO PluginFour()
        {
            return new PluginDO
            {
                Name = "AzureSqlServer",
                Endpoint = "AzureSqlServer",
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static PluginDO PluginFive()
        {
            return new PluginDO
            {
                Name = "DocuSign",
                Endpoint = "localhost",
                PluginStatus = PluginStatus.Active
            };
        }
    }
}
