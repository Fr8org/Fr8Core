using System;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using StructureMap;
using Hub.Infrastructure;
using PlanDirectory.Infrastructure;

namespace PlanDirectory.Controllers.Api
{
    public class AuthenticationController : ApiController
    {
        private readonly IAuthTokenManager _authTokenManager;


        public AuthenticationController()
        {
            _authTokenManager = ObjectFactory.GetInstance<IAuthTokenManager>();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [PlanDirectoryHMACAuthenticate]
        public IHttpActionResult Token()
        {
            var fr8UserId = User.Identity.GetUserId();
            var token = _authTokenManager.CreateToken(Guid.Parse(fr8UserId));

            return Ok(new { token });
        }

        [HttpGet]
        [ActionName("is_authenticated")]
        public IHttpActionResult IsAuthenicated()
        {
            var authenticated = User.Identity.IsAuthenticated;
            return Ok(new { authenticated });
        }
    }
}