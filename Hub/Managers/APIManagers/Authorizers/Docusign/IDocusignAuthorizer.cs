using System.Threading.Tasks;

namespace Hub.Managers.APIManagers.Authorizers.Docusign
{
    public interface IDocusignAuthorizer : IOAuthAuthorizer
    {
        Task ObtainAccessTokenAsync(string userId, string userName, string password);
    }
}