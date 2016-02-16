using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using System.Collections.Generic;
using Data.Crates;
using Hub.Managers;
using StructureMap;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityDTO TestActionDTO1()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1(),
            };
        }
        public static ActivityDTO TestActionDTO2()
        {
            ActivityDTO curActionDTO = new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1(),
            };

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActionDTO))
            {
                crateStorage.Add(CreateStandardConfigurationControls());
            }

            return curActionDTO;
        }

        public static ActivityDTO TestActionDTO3()
        {
            ActivityDTO curActionDTO = new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1()
            };

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActionDTO))
            {
                var controls = CreateStandardConfigurationControls();
                crateStorage.Add(controls);
                crateStorage.Add(controls);
            }

            return curActionDTO;
        }

        public static ActivityDTO CreateStandardDesignTimeFields()
        {
            ActivityDTO curActionDTO = new ActivityDTO();
            var curCratesDTO = FixtureData.TestCrateDTO2();
            
            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActionDTO))
            {
                crateStorage.AddRange(curCratesDTO);
            }

            return curActionDTO;
        }

        public static ActivityDTO TestActionDTOForSalesforce()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateSalesforce()
            };
        }

        public static ActivityDTO TestActionDTOForSendGrid()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateSendGrid()
            };
        }
        public static ActivityDTO TestActionDTOSelectFr8ObjectInitial()
        {
            ActivityDTO curActionDTO = new ActivityDTO()
            {
                ActivityTemplate = FixtureData.ActivityTemplateDTOSelectFr8Object(),
            };
            // curActionDTO.CrateStorage.CrateDTO.Add(CreateStandardConfigurationControls());

            return curActionDTO;
        }
        public static ActivityDTO TestActionDTOSelectFr8ObjectFollowup(string selected)
        {
            ActivityDTO curActionDTO = new ActivityDTO()
            {
                ActivityTemplate = FixtureData.ActivityTemplateDTOSelectFr8Object(),
            };

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActionDTO))
            {
                crateStorage.Add(CreateStandardConfigurationControlSelectFr8Object(selected));
            }
            return curActionDTO;
        }
		
    }
}