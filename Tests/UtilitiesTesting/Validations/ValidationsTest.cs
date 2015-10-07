using System;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;

namespace UtilitiesTesting.Validations
{
    [TestFixture]
    public class ValidationsTest : BaseTest
    {
        [Test]
        [Category("PluginUtilities.Infrastructure.Validations.ValidateDTO")]
        public void Validations_ValidateDTOWithValidString()
        {
            const string availableActions = "{'type_name':'write to azure sql server','version':4.3}";
            var result = terminal_base.Infrastructure.Validations.ValidateDtoString<ActionTypeListDTO>(availableActions);

            Assert.IsTrue(result);
        }

        [Test]
        [Category("PluginUtilities.Infrastructure.Validations.ValidateDTO")]
        [ExpectedException(typeof(ArgumentException))]
        public void Validations_ValidateDTOWithInvalidString()
        {
            const string availableActions = "some invalid string";
            terminal_base.Infrastructure.Validations.ValidateDtoString<ActionTypeListDTO>(availableActions);
        }
    }
}