using System.Collections.Generic;

namespace terminalDocuSign.DataTransferObjects
{
    /// <summary>
    /// Contains one row data from specific DocuSign Tab 
    /// </summary>
    public class DocuSignTabDTO
    {
        public string Name { get; set; }

        public string TabId { get; set; }

        public string RecipientId { get; set; }

        public string EnvelopeId { get; set; }

        public string Value { get; set; }

        public int DocumentId { get; set; }

        public string Fr8DisplayType { get; set; }

        public string RoleName { get; set; }

        public string TabName { get; set; }

        public string TabLabel { get; set; }

        public string Type { get; set; }
    }

    /// <summary>
    /// Wrapper object used for DocuSign extracted data from radioButtonGroupTabs and list tabs
    /// </summary>
    public class DocuSignMultipleOptionsTabDTO : DocuSignTabDTO
    {
        public DocuSignMultipleOptionsTabDTO()
        {
            Items = new List<DocuSignOptionItemTabDTO>();
        }

        public List<DocuSignOptionItemTabDTO> Items { get; set; }
    }

    /// <summary>
    /// Wrapper object for radio items inside radio button groups and list item inside dropdown lists
    /// </summary>
    public class DocuSignOptionItemTabDTO
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}