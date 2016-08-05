namespace Data.Entities
{
    public class FileDO : BaseObject
    {
        public int Id { get; set; }

        public string CloudStorageUrl { get; set; }

        public string OriginalFileName { get; set; }

        public string DockyardAccountID { get; set; }
    }
}
