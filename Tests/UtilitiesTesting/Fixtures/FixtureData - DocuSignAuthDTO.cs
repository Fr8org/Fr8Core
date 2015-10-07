﻿using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using pluginDocuSign.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static DocuSignAuthDTO TestDocuSignAuthDTO1()
        {
            return new DocuSignAuthDTO() { Email = "64684b41-bdfd-4121-8f81-c825a6a03582", ApiPassword = "H1e0D79tpJ3a/7klfhPkPxNMcOo=" };
        }
    }
}
