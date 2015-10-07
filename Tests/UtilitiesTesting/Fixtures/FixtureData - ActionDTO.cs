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
        public static ActionDTO TestActionDTO1()
        {
            return new ActionDTO()
            {
                Name = "test action type",
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1(),
                AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(TestDocuSignAuthDTO1()) }
            };
        }
        public static ActionDTO TestActionDTO2()
        {
            ActionDTO curActionDTO = new ActionDTO()
            {
                Name = "test action type",
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1(),
                AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(TestDocuSignAuthDTO1()) }
            };
            curActionDTO.CrateStorage.CrateDTO.Add(CreateStandardConfigurationControls());

            return curActionDTO;
        }

        public static ActionDTO TestActionDTO3()
        {
            ActionDTO curActionDTO = new ActionDTO()
            {
                Name = "test action type",
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1()
            };
            curActionDTO.CrateStorage.CrateDTO.Add(CreateStandardConfigurationControls());
            var configurationFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(curActionDTO.CrateStorage.CrateDTO[0].Contents);

            curActionDTO.CrateStorage.CrateDTO.Add(CreateEventSubscriptionCrate(configurationFields));

            return curActionDTO;
        }

        public static ActionDTO CreateStandardDesignTimeFields()
        {
            ActionDTO curActionDTO = new ActionDTO();
            List<CrateDTO> curCratesDTO = FixtureData.TestCrateDTO2();
            curActionDTO.CrateStorage.CrateDTO.AddRange(curCratesDTO);
            curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(TestDocuSignAuthDTO1()) };
            return curActionDTO;
        }

        public static ActionDTO TestActionDTOForSalesforce()
        {
            return new ActionDTO()
            {
                Name = "test salesforce action",
                ActivityTemplate = FixtureData.TestActivityTemplateSalesforce()
            };
        }
    }
}
