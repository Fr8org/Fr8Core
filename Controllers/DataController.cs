using System;
using System.Web.Mvc;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Services;

namespace HubWeb.Controllers
{
    public class DataController : Controller
    {
        readonly EmailAddress _emailAddress;
        private IConfigRepository _configRepository;

        public DataController()
        {
            _emailAddress = ObjectFactory.GetInstance<EmailAddress>();
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
        }

        [HttpPost]
        public ActionResult ValidateEmailAddress(string emailString)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailAddressDO emailAddressDO = _emailAddress.ConvertFromString(emailString, uow);
                    if (!(RegexUtilities.IsValidEmailAddress(_configRepository, emailAddressDO.Address)))
                        return Json("Invalid email format");
                    else
                        return Json(true);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

    }
}