using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Newtonsoft.Json;

namespace Data.Entities
{
    public class EnvelopeDO : BaseObject, IMailerDO
    {
        public const string MailHandler = "Gmail";

        public EnvelopeDO()
        {
            MergeData = new Dictionary<string, object>();
        }

        [NotMapped]
        public IDictionary<String, Object> MergeData { get; set; }

        [NotMapped]
        IEmailDO IMailerDO.Email
        {
            get { return Email; }
            set { Email = (EmailDO)value; }
        }

        public string Footer { get; set; }

        [Key]
        public int Id { get; set; }
        public string Handler { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }

        [ForeignKey("Email"), Required]
        public int? EmailID { get; set; }
        public virtual EmailDO Email { get; set; }

        /// <summary>
        /// Do not manual modify this value. Use MergeData
        /// </summary>
        [Column("MergeData")]
        public string MergeDataString
        {
            get
            {
                return JsonConvert.SerializeObject(MergeData);
            }
            set
            {
                MergeData = JsonConvert.DeserializeObject<Dictionary<String, Object>>(value);
            }
        }

        /*
                    public EnvelopeState Status { get; set; } //renamed to envelopestatus because it will hide the parent status property
                    public enum EnvelopeState
                    {
                        Any,
                        Created,
                        Sent,
                        Delivered,
                        Signed,
                        Completed,
                        Declined,
                        Voided,
                        Deleted
                    };
        */
    }
}
