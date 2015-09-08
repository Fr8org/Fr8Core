namespace pluginDocuSign.Infrastructure
{
    public class DocuSignAccount : DocuSign.Integrations.Client.Account
    {
        public static DocuSignAccount Create(DocuSign.Integrations.Client.Account account)
        {
            return AutoMapper.Mapper.Map<DocuSignAccount>(account);
        }
    }
}