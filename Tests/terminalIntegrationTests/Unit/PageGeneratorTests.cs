using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Repositories;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Services.PlanDirectory;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace terminalIntegrationTests.Unit
{
    public class PageGeneratorTests
    {
        [Test]
        [Ignore]
        public void ShouldGeneratePagesFromTags()
        {
            // Fixture setup
            Fixture fixture = new Fixture();

            var wsDTO1 = fixture.Build<ActivityCategoryDTO>()
                .With(x => x.Id, Guid.Parse("9150ADCC-9996-44BF-A497-8993E51F5D37"))
                .With(x => x.Name, "Excel")
                .With(x => x.IconPath, @"/Content/icons/web_services/ms-excel-icon-64x64.png")
                .Create();
            var wsDTO2 = fixture.Build<ActivityCategoryDTO>()
                .With(x => x.Id, Guid.Parse("6ABAD1CE-A41A-4098-95B9-9C8F0B589738"))
                .With(x => x.Name, "Slack")
                .With(x => x.IconPath, @"/Content/icons/web_services/slack-icon-64x64.png")
                .Create();

            var tag1 = new WebServiceTemplateTag(new List<ActivityCategoryDTO> { wsDTO1 });
            var tag2 = new WebServiceTemplateTag(new List<ActivityCategoryDTO> { wsDTO2, wsDTO1 });

            var pageDefinitionRepositoryStub = new Mock<IPageDefinitionRepository>();

            var pd1 = fixture.Build<PageDefinitionDO>()
                .Without(x => x.UrlString)
                .With(x => x.Tags, new[] { "slack", "excel" })
                .Create();
            pageDefinitionRepositoryStub.Setup(x => x.GetAll()).Returns(new List<PageDefinitionDO>
            {
                pd1
            });
            var storage = new TemplateTagStorage();
            storage.WebServiceTemplateTags.Add(tag1);
            storage.WebServiceTemplateTags.Add(tag2);
            //var sut = new PageGenerator(pageDefinitionRepositoryStub.Object);

            //sut.Generate(storage, new PlanTemplateCM() { Name = "foo", Description = null });
        }
    }
}
