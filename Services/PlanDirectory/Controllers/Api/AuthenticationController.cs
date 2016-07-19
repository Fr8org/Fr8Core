using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using StructureMap;
using Data.Interfaces;
using Data.Infrastructure.StructureMap;
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
        public IHttpActionResult LogIn([FromUri]string username, [FromUri]string password)
        {
            Request.GetOwinContext().Authentication.SignOut();

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fr8AccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (fr8AccountDO != null)
                {
                    var passwordHasher = new PasswordHasher();
                    if (passwordHasher.VerifyHashedPassword(fr8AccountDO.PasswordHash, password) ==
                        PasswordVerificationResult.Success)
                    {
                        var security = ObjectFactory.GetInstance<ISecurityServices>();
                        var identity = security.GetIdentity(uow, fr8AccountDO);
                        Request.GetOwinContext()
                            .Authentication
                            .SignIn(
                                new AuthenticationProperties
                                {
                                    IsPersistent = true
                                },
                                identity
                            );

                        return Ok();
                    }
                }
            }

            return StatusCode(HttpStatusCode.Forbidden);
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

        [HttpGet]
        [ActionName("is_privileged")]
        public IHttpActionResult IsPrivileged()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Ok(new { privileged = false });
            }

            var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");

            return Ok(new { privileged });
        }
    }
}