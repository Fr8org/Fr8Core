using System.Web.Mvc;
using Data.Interfaces;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;

namespace KwasantWeb.Controllers
{
    public class QuestionController : Controller
    {
        [HttpPost]
        public ActionResult EditTimeslots(int? calendarID, int? negotiationID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                CalendarDO calendarDO = null;
                if (calendarID != null)
                    calendarDO = uow.CalendarRepository.FindOne(c => c.Id == calendarID);

                if (calendarDO == null)
                {
                    //The rest of the calendar will be updated later
                    calendarDO = new CalendarDO
                    {
                        Name = "Negotiation Caledar",
                        NegotiationID = negotiationID,
                        OwnerID = this.GetUserId()
                    };
                    uow.CalendarRepository.Add(calendarDO);
                    uow.SaveChanges();
                }
                return Json(calendarDO.Id);
            }
        }
    }
}