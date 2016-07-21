using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Segment.Model;
using StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Interfaces;

namespace HubWeb.Controllers
{
    public class TokenAuthController : Controller
    {
	    private readonly ITime _time;

	    public TokenAuthController()
	    {
		    _time = ObjectFactory.GetInstance<ITime>();
	    }

        public ActionResult Index(string token)
        {
            Guid tokenId;

            if (!Guid.TryParse(token, out tokenId))
            {
                throw new HttpException(500, "Invalid token Id.");
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(tokenId);
                
				if (validToken == null)
                    throw new HttpException(404, "Authorization token not found.");

	            DateTime currentTime = _time.CurrentDateTime();

				if (validToken.ExpiresAt < currentTime)
                    throw new HttpException(404, "Authorization token expired.");

                // Auth token are cached, so navigational properties will not work
                var user = uow.UserRepository.GetByKey(validToken.UserID);

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, user);

                if (!String.IsNullOrEmpty(validToken.SegmentTrackingEventName))
                {
                    Properties segmentProps = null;
                    if (!String.IsNullOrEmpty(validToken.SegmentTrackingProperties))
                    {
                        segmentProps = new Properties();
                        var trackingProperties = JsonConvert.DeserializeObject<Dictionary<String, Object>>(validToken.SegmentTrackingProperties);
                        foreach (var prop in trackingProperties)
                            segmentProps.Add(prop.Key, prop.Value);
                    }

                    ObjectFactory.GetInstance<ITracker>().Track(user, validToken.SegmentTrackingEventName, segmentProps);
                }

                return Redirect(validToken.RedirectURL);
            }
        }
	}
}