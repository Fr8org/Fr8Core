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
             return new DocuSignAuthDTO() { Email = "alex@edelstein.org", ApiPassword = "foobar" };
        }
    }
}
