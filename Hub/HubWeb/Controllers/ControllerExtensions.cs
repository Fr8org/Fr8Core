using System.Web.Mvc;
using StructureMap;
using Data.Infrastructure.StructureMap;

namespace HubWeb.Controllers
{
    public static class ControllerExtensions
    {
        public static string GetUserId(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser();
        }
        public static string GetUserName(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().GetUserName();
        }
        public static string[] GetRoleNames(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().GetRoleNames();
        }
        public static bool UserIsAuthenticated(this Controller controller)
        {
            return ObjectFactory.GetInstance<ISecurityServices>().IsAuthenticated();
        }
        public static void Logout(this Controller controller)
        {
            ObjectFactory.GetInstance<ISecurityServices>().Logout();
        }
    }
}