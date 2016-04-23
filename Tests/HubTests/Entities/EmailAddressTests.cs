using System;
using System.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Utilities;
using UtilitiesTesting;

namespace HubTests.Entities
{
    [TestFixture,Ignore]
    [Category("EmailAddress")]
    public class EmailAddressTests : BaseTest
    {
        [Test, ExpectedException(ExpectedMessage = "Duplicate values for 'Address' on 'EmailAddressDO' are not allowed. Duplicated value: 'rjrudman@gmail.com'")]
        public void TestDuplicateEmailRejected()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.EmailAddressRepository.Add(new EmailAddressDO("rjrudman@gmail.com"));
                uow.EmailAddressRepository.Add(new EmailAddressDO("rjrudman@gmail.com"));
                uow.SaveChanges();
            }
        }

        [Test]
        [Ignore]
        public void TestBasicEmail()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestMultipleBasicEmails()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.com,otheremail@gmail.com");

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);

            Assert.AreEqual(String.Empty, result[1].Name);
            Assert.AreEqual("otheremail@gmail.com", result[1].Email);
        }

        [Test]
        [Ignore]
        public void TestEmailWithName()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestEmailWithFullName()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert Robert>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert Robert", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("<Robert23>rjrudman@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Robert23", result[0].Name);
            Assert.AreEqual("rjrudman@gmail.com", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestEmailNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman23@gmail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman23@gmail.com", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestDomainlNameWithNumbers()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@g23mail.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@g23mail.com", result[0].Email);
        }

        [Test]
        public void TestInvalidTLD_Short()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("rjrudman@gmail.c");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [Ignore]
        public void TestComplexTLD()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            //This is valid TLD - as per http://data.iana.org/TLD/tlds-alpha-by-domain.txt
            var result = emailAddress.ExtractFromString("rjrudman@gmail.XN--CLCHC0EA0B2G2A9GCD");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("rjrudman@gmail.xn--clchc0ea0b2g2a9gcd", result[0].Email);
        }

        [Test]
        [Ignore]
        public void TestEmailAddressDOCreated()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, uow.EmailAddressRepository.GetQuery().Count());
                emailAddress.GetEmailAddresses(uow, "rjrudman@gmail.com");
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
            }
        }

        [Test]
        [Ignore]
        public void TestEmailAddressDODuplicateNotCreated()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Assert.AreEqual(0, uow.EmailAddressRepository.GetQuery().Count());
                uow.EmailAddressRepository.Add(new EmailAddressDO
                {
                    Address = "rjrudman@gmail.com",
                    Name = "rjrudman@gmail.com"
                });
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
                emailAddress.GetEmailAddresses(uow, "rjrudman@gmail.com");
                uow.SaveChanges();
                Assert.AreEqual(1, uow.EmailAddressRepository.GetQuery().Count());
            }
        }

        [Test]
        [Ignore]
        public void TestCorruptEmailNotParsed()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("hq@kwasant.comalex@edelstein.org");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("hq@kwasant.comalex", result[0].Email); //Technically a valid email. What we're really testing for, though, is that '@edelstein.org' is not parsed as a seperate email
        }

        [Test]
        [Ignore]
        public void TestDashInDomain()
        {
            var emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            var result = emailAddress.ExtractFromString("DGerrard@gerrard-cox.com");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(String.Empty, result[0].Name);
            Assert.AreEqual("dgerrard@gerrard-cox.com", result[0].Email);
        }

        [Test]
        public void TestIllegalSurroundingCharactersInvalid()
        {
            var ru = new RegexUtilities();
            Assert.False(ru.IsValidEmailAddress("'rjrudman@gmail.com'"));
        }

        [Test]
        public void TestEmailNameWithPeriod()
        {
            var ru = new RegexUtilities();
            Assert.True(ru.IsValidEmailAddress("rj.rudman@gmail.com"));
        }

        [Test]
        public void TestEmailNameWithDash()
        {
            var ru = new RegexUtilities();
            Assert.True(ru.IsValidEmailAddress("rj-rudman@gmail.com"));
        }

        [Test]
        public void TestEmailDomainWithPeriod()
        {
            var ru = new RegexUtilities();
            Assert.True(ru.IsValidEmailAddress("rjrudman@gmail.net.au"));
        }

        [Test]
        public void TestEmailUnderscore()
        {
            var ru = new RegexUtilities();
            Assert.True(ru.IsValidEmailAddress("br_notify@kwasant.com"));
        }

        [Test]
        public void TestEmailWithTrailingSpace()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress("someEmail@gmail.com "); 
                Assert.NotNull(emailAddress);
            }
        }
    }
}
