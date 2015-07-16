using System.Threading.Tasks;

namespace Core.Managers.APIManagers.Authorizers.Docusign
{
    public interface IDocusignAuthorizer : IOAuthAuthorizer
    {
        Task ObtainAccessTokenAsync(string userId, string userName, string password);
    }
}