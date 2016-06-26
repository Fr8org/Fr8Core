using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using StructureMap;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static ActivityDTO TestActivityDTO1()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateDTO1(),
            };
        }
        public static ActivityDTO TestActivityDTO2()
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

        public static ActivityDTO TestActivityDTO3()
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
        
        public static ActivityDTO TestActivityDTOForSalesforce()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateSalesforce()
            };
        }

        public static ActivityDTO TestActivityDTOForSendGrid()
        {
            return new ActivityDTO()
            {
                ActivityTemplate = FixtureData.TestActivityTemplateSendGrid()
            };
        }
        public static ActivityDTO TestActivityDTOSelectFr8ObjectInitial()
        {
            ActivityDTO curActionDTO = new ActivityDTO()
            {
                ActivityTemplate = FixtureData.ActivityTemplateDTOSelectFr8Object(),
            };
            // curActionDTO.CrateStorage.CrateDTO.Add(CreateStandardConfigurationControls());

            return curActionDTO;
        }
        public static ActivityDTO TestActivityDTOSelectFr8ObjectFollowup(string selected)
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