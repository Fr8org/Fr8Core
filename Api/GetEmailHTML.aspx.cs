using System;
using System.Linq;
using System.Web.UI;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace KwasantWeb.Api
{
    public partial class GetEmailHTML : Page
    {
        private EmailDO m_EmailRow;
        private String booker;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int emailID;
                if (int.TryParse(Request["EmailID"], out emailID))
                {
                    LoadEmailRow(emailID);
                }
            }
        }

        private void LoadEmailRow(int emailID)
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            m_EmailRow = uow.EmailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
            string bookerId = uow.BookingRequestRepository.GetByKey(emailID).BookerID;
            if (bookerId != null)
            {
                booker = uow.UserRepository.GetByKey(bookerId).EmailAddress.Address;
            }
            else { booker = "none"; }
        }

        protected String GetBooker()
        {
            return booker;
        }

        protected String GetEmailId()
        {
            return m_EmailRow.Id.ToString();
        }

        protected String GetEmailSubject()
        {
            return m_EmailRow.Subject;
        }

        protected String GetTo()
        {
            if (!m_EmailRow.To.Any())
                return "None";
            return String.Join(", ", m_EmailRow.To.Select(a => a.Address));
        }

        protected String GetCC()
        {
            if (!m_EmailRow.To.Any())
                return "None";
            return String.Join(", ", m_EmailRow.CC.Select(a => a.Address));
        }

        protected String GetBCC()
        {
            if (!m_EmailRow.To.Any())
                return "None";
            return String.Join(", ", m_EmailRow.BCC.Select(a => a.Address));
        }

        protected String GetEmail()
        {
            if (m_EmailRow.From == null || String.IsNullOrEmpty(m_EmailRow.From.Address))
                return "Unknown";

            return m_EmailRow.From.Address;
        }

        protected String GetFromPerson()
        {
            if (m_EmailRow.From == null || String.IsNullOrEmpty(m_EmailRow.From.Name))
                return "Unknown";

            return m_EmailRow.From.Name;
        }

        protected String GetAttachments()
        {
            if (m_EmailRow.Attachments != null && m_EmailRow.Attachments.Any())
            {
                const string fileViewURLStr = "/Api/GetAttachment.ashx?AttachmentID={0}";

                var attachmentInfo = String.Join("<br />",
                            m_EmailRow.Attachments.Select(
                                attachment =>
                                "<a href='" + String.Format(fileViewURLStr, attachment.Id) + "' target='" +
                                attachment.OriginalName + "'>" + attachment.OriginalName + "</a>"));

                return attachmentInfo;
            }
            
            return "No attachments (TO DO)";
        }

        protected String GetContent()
        {
            var content = m_EmailRow.HTMLText;
            return content;
        }

    }
}