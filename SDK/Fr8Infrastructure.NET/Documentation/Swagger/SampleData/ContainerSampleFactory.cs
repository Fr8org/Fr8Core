using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ContainerSampleFactory : ISwaggerSampleFactory<ContainerDTO>
    {
        private readonly ISwaggerSampleFactory<ValidationErrorsDTO> _validationErrorSampleFactory;
        public ContainerSampleFactory(ISwaggerSampleFactory<ValidationErrorsDTO> validationErrorSampleFactory)
        {
            _validationErrorSampleFactory = validationErrorSampleFactory;
        }

        public ContainerDTO GetSampleData()
        {
            return new ContainerDTO
            {
                Id = Guid.Parse("713DF7EE-57CB-41B9-AD0F-A0D644FC8D2F"),
                Name = "Container Name",
                ValidationErrors = new Dictionary<Guid, ValidationErrorsDTO>
                {
                    { Guid.Parse("5BF52A90-2AB1-4492-B63F-3785B9A5731C"), _validationErrorSampleFactory.GetSampleData() }
                },
                CreateDate = DateTimeOffset.Now,
                CurrentActivityResponse = ActivityResponse.Success,
                CurrentClientActivityName = "Build_Message_v1",
                CurrentPlanType = PlanType.RunOnce,
                LastUpdated = DateTimeOffset.Now,
                PlanId = Guid.Parse("56823F8E-6F5C-4444-BEFE-7D4642851762")
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}