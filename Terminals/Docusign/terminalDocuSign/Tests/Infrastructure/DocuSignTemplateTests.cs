namespace terminalDocuSign.Tests.Infrastructure
{
    //[TestFixture]
    //[Category("DocuSignTemplate")]
    //public class DocuSignTemplateTests : BaseTest
    //{
    //    private readonly string TEMPLATE_WITH_ROLES_ID = "9a318240-3bee-475c-9721-370d1c22cec4";
    //    private readonly string TEMPLATE_WITH_USER_FIELDS_ID = "9a318240-3bee-475c-9721-370d1c22cec4";
    //    private DocuSignTemplate _docusignTemplate;

    //    [SetUp]
    //    public override void SetUp()
    //    {
    //        base.SetUp();
    //        TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();
    //        TerminalDocuSignMapBootstrapper.ConfigureDependencies(DependencyType.LIVE);
    //        CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

    //        // _docusignTemplate = ObjectFactory.GetInstance<IDocuSignTemplate>();
    //        _docusignTemplate = new DocuSignTemplate();
    //        _docusignTemplate.Login = new DocuSignPackager().Login();
    //    }
    //    [Test]
    //    public void GetUserFields_ExistsTempate_ShouldBeOk()
    //    {
    //        var docuSignTemplateDTO = _docusignTemplate.GetTemplateById(TEMPLATE_WITH_USER_FIELDS_ID);
    //        var userFields = _docusignTemplate.GetUserFields(docuSignTemplateDTO);
    //        var t1 = userFields.Where(x => x == "CustomField1_Value1").FirstOrDefault();
    //        var t2 = userFields.Where(x => x == "CustomField2_Value2").FirstOrDefault();
    //        Assert.AreEqual(2, userFields.Count);
    //        Assert.NotNull(t1);
    //        Assert.NotNull(t2);
    //    }
    //    [Test]
    //    public void GettingRecipientsViaEnvelopeDataProperty_ExistsTemplate_ShouldBeOk()
    //    {
    //        var docuSignTemplateDTO = _docusignTemplate.GetTemplateById(TEMPLATE_WITH_ROLES_ID);
    //        var signers = docuSignTemplateDTO.EnvelopeData.recipients.signers;
    //        var carbonCopies = docuSignTemplateDTO.EnvelopeData.recipients.carbonCopies;

    //        Assert.AreEqual(3, signers.Length);
    //        Assert.NotNull(signers.Where(x => x.roleName == "Director").SingleOrDefault());
    //        Assert.NotNull(signers.Where(x => x.email == "reasyu@gmail.com").SingleOrDefault());

    //        Assert.NotNull(signers.Where(x => x.roleName == "President").SingleOrDefault());
    //        Assert.NotNull(signers.Where(x => x.email == "docusign_developer@dockyard.company").SingleOrDefault());

    //        Assert.NotNull(signers.Where(x => x.roleName == "Project Manager").SingleOrDefault());
    //        Assert.NotNull(signers.Where(x => x.email == "joanna@fogcitymail.com").SingleOrDefault());

    //        Assert.AreEqual(1, carbonCopies.Length);
    //        Assert.NotNull(carbonCopies.Where(x => x.roleName == "Vise President").SingleOrDefault());
    //        Assert.NotNull(carbonCopies.Where(x => x.email == "reasyu@yandex.ru").SingleOrDefault());
    //    }
    //    [Test]
    //    public void GetTemplateById_NonExistsTemplate_ExpectedException()
    //    {
    //        var ex = Assert.Throws<InvalidOperationException>(() => _docusignTemplate.GetTemplateById(Guid.NewGuid().ToString()));
    //    }
    //    [Test]
    //    public void GetTemplateById_ExistsTemplate_ShouldBeOk()
    //    {
    //        DocuSignTemplateDTO docuSignTemplateDTO = _docusignTemplate.GetTemplateById(TEMPLATE_WITH_ROLES_ID);

    //        Assert.AreEqual(TEMPLATE_WITH_ROLES_ID, docuSignTemplateDTO.Id);
    //    }
    //    [Test]
    //    public void GetTemplatesReturnsValues()
    //    {
    //        var templatesInfos = _docusignTemplate.GetTemplates();
    //        Assert.AreNotSame(templatesInfos.Count, 0);
    //    }
    //}
}