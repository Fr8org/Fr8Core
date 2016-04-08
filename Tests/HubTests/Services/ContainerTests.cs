using System;
using Data.Interfaces;
using Hub.Exceptions;
using HubTests.Fixtures;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;

namespace HubTests.Services
{
    [TestFixture]
    [Category("Container")]
    public class ContainerTests : BaseTest
    {
        [Test, Ignore]
        public void ProcessCurrentActivityResponse_WhenErrorExists_ThrowsErrorResponseExceptionAndUsesItsErrorMessage()
        {
            var container = FixtureData.EmptyContainer();
            var containerDO = FixtureData.EmptyContainerDO();
            var activityResponseDTO = FixtureData.ErrorActivityResponseDTOWithErrorMessage();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                try
                {
                    //container.ProcessCurrentActivityResponse(uow, containerDO, activityResponseDTO).Wait();
                    Assert.Fail("Exception must be thrown when processing ActivityResponse of error type");
                }
                catch (AggregateException ex)
                {
                    var actualException = ex.InnerExceptions[0] as ErrorResponseException;
                    Assert.IsNotNull(actualException, $"Exception should be of type {nameof(ErrorResponseException)}");
                    Assert.AreEqual(FixtureData.ErrorMessage, actualException.Message, "Exception message should match that stored in activity response");
                }
            }
        }

        [Test, Ignore]
        public void ProcessCurrentActivityResponse_WhenErrorDoesntExist_ThrowsErrorResponseExceptionWithEmptyMessage()
        {
            var container = FixtureData.EmptyContainer();
            var containerDO = FixtureData.EmptyContainerDO();
            var activityResponseDTO = FixtureData.ErrorActivityResponseDTOWithoutErrorMessage();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                try
                {
                    //container.ProcessCurrentActivityResponse(uow, containerDO, activityResponseDTO).Wait();
                    Assert.Fail("Exception must be thrown when processing ActivityResponse of error type");
                }
                catch (AggregateException ex)
                {
                    var actualException = ex.InnerExceptions[0] as ErrorResponseException;
                    Assert.IsNotNull(actualException, $"Exception should be of type {nameof(ErrorResponseException)}");
                    Assert.IsEmpty(actualException.Message, "Exception message should be empty when activity response doesn't contain error object");
                }
            }
        }
    }
}
