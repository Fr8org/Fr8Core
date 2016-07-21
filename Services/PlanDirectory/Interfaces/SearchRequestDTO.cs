namespace PlanDirectory.Interfaces
{
    public class SearchRequestDTO
    {
        public string Text { get; set; }
        public int PageStart { get; set; }
        public int PageSize { get; set; }
    }
}