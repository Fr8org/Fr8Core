using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using Hub.Services;
using Utilities;

namespace HubWeb.Controllers
{
    public class DataController : Controller
    {
        readonly EmailAddress _emailAddress;
        
        public DataController()
        {
            _emailAddress = ObjectFactory.GetInstance<EmailAddress>();
        }

        [HttpPost]
        public ActionResult ValidateEmailAddress(string emailString)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailAddressDO emailAddressDO = _emailAddress.ConvertFromString(emailString, uow);
                    var ru = new RegexUtilities();
                    if (!(ru.IsValidEmailAddress(emailAddressDO.Address)))
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