using System.Collections.Generic;

namespace Fr8Data.DataTransferObjects
{
    public class PageDefinitionDTO
    {
        public string Title { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string AuthorUrl { get; set; }
        //TODO: Create Value Object
        public string PageName { get; set; }
    }
}