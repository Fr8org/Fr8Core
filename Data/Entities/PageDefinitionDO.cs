using System;
using System.Collections.Generic;
using Data.Utility;

namespace Data.Entities
{
    public class PageDefinitionDO : BaseObject
    {
        public string Title { get; set; }
        public IEnumerable<TagDO> Tags { get; set; }
        public Uri Url { get; set; }
        public string Type { get; set; }
        // FK
        public string Author { get; set; }
        public string Description { get; set; }
        public Uri AuthorUrl { get; set; }
        //TODO: Create Value Object
        public string PageName { get; set; }
    }
}