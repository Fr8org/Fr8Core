using System;
using Data.Interfaces;
using StructureMap;

namespace KwasantWeb.Api
{
    public partial class EventResponse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var userID = Request.QueryString["userID"];
            if (String.IsNullOrEmpty(userID))
            {
                userID = "11aa11ba-d572-4f10-8009-fecf55eafc3e";
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserAgentInfoRepository.TrackRequest(userID, Request);
                uow.SaveChanges();
            }
        }
    }
}