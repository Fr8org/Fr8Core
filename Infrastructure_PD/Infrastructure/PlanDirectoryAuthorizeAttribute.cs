//using System.Web;
//using System.Web.Mvc;
//using Hub.Managers;

//namespace PlanDirectory.Infrastructure
//{
//    public class PlanDirectoryAuthorizeAttribute : DockyardAuthorizeAttribute
//    {
//        public PlanDirectoryAuthorizeAttribute(params string[] roles) : base(roles)
//        {
//        }

//        protected override string BuildRedirectUrl(AuthorizationContext context)
//        {
//            return VirtualPathUtility.ToAbsolute("~/Reauthenticate");
//        }
//    }
//}