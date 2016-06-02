using System.Web;
using System.Web.Mvc;
using StructureMap;
using Data.Interfaces;
using Data.Infrastructure.StructureMap;
using PlanDirectory.Infrastructure;

namespace PlanDirectory.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthTokenManager _authTokenManager;


        public HomeController()
        {
            _authTokenManager = ObjectFactory.GetInstance<IAuthTokenManager>();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AuthenticateByToken(string token)
        {
            try
            {
                var fr8AccountId = _authTokenManager.GetFr8AccountId(token);
                if (!fr8AccountId.HasValue)
                {
                    return Redirect(VirtualPathUtility.ToAbsolute("~/Reauthenticate"));
                }

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var fr8AccountDO = uow.UserRepository.GetByKey(fr8AccountId.Value.ToString());

                    var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
                    securityServices.Logout();
                    securityServices.Login(uow, fr8AccountDO);

                    return Redirect(VirtualPathUtility.ToAbsolute("~/"));
                }
            }
            catch (System.Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);

                return Content(sb.ToString());
            }
        }

        [HttpGet]
        public ActionResult Reauthenticate()
        {
            return View();
        }
    }
}
