using System;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;
using UtilitiesTesting;

namespace HubTests.Validations
{
    [TestFixture]
    [Category("Validations")]
    public class ValidationsTest :  BaseTest
    {
        [Test]
        public void Validations_ValidateDTOWithValidString()
        {
            const string availableActions = "{'type_name':'write to azure sql server','version':4.3}";
            var result = TerminalBase.Infrastructure.Validations.ValidateDtoString<ActivityTypeListDTO>(availableActions);

            Assert.IsTrue(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Validations_ValidateDTOWithInvalidString()
        {
            const string availableActions = "some invalid string";
            TerminalBase.Infrastructure.Validations.ValidateDtoString<ActivityTypeListDTO>(availableActions);
        }
    }
}