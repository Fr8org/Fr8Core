
namespace Data.Entities
{
    public class FileDO : BaseDO
    {
        public int Id { get; set; }

        public int? DocuSignTemplateID { get; set; }

        public int? DocuSignEnvelopeID { get; set; }

        public string CloudStorageUrl { get; set; }
    }
}
