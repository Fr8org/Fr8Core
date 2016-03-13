using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using Utilities.Configuration.Azure;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class MonitorAllDocuSignEvents_Tests
    {
        // private const string UserAccountName = "y.gnusin@gmail.com";
        private const string UserAccountName = "IntegrationTestUser1";
        private const int AwaitPeriod = 120000;
        private const string TemplateName = "Medical_Form_v2";

        private const string ToEmail = "freight.testing@gmail.com";
        private const string DocuSignEmail = "freight.testing@gmail.com";
        private const string DocuSignApiPassword = "I6HmXEbCxN";


        [SetUp]
        public void Init()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
        }

        [Test]
        public async void Test_MonitorAllDocuSignEvents_Plan()
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testAccount = unitOfWork.UserRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.UserName == UserAccountName);

                var mtDataCountBefore = unitOfWork.MultiTenantObjectRepository
                    .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id.ToString())
                    .Count();

                await SendDocuSignTestEnvelope();

                await Task.Delay(AwaitPeriod);

                var mtDataCountAfter = unitOfWork.MultiTenantObjectRepository
                    .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id.ToString())
                    .Count();

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter);
            }
        }

        private async Task SendDocuSignTestEnvelope()
        {
            var endpoint = CloudConfigurationManager.GetSetting("endpoint");

            var authManager = new DocuSignAuthentication();
            var password = await authManager
                .ObtainOAuthToken(DocuSignEmail, DocuSignApiPassword, endpoint);

            var templateManager = new DocuSignTemplate();
            var template = templateManager
                .GetTemplateNames(
                    DocuSignEmail,
                    password
                )
                .FirstOrDefault(x => x.Name == TemplateName);

            if (template == null)
            {
                throw new ApplicationException(string.Format("Unable to extract {0} template from DocuSign", TemplateName));
            }

            var loginInfo = DocuSignService.Login(
                DocuSignEmail,
                password
            );

            var rolesList = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Tags = "recipientId:72179268",
                    Key = "role name",
                    Value = ToEmail
                },
                new FieldDTO()
                {
                    Tags = "recipientId:72179268",
                    Key = "role email",
                    Value = ToEmail
                }
            };

            var fieldsList = new List<FieldDTO>();
            
            DocuSignService.SendAnEnvelopeFromTemplate(
                loginInfo,
                rolesList,
                fieldsList,
                template.Id
            );
        }
    }
}
