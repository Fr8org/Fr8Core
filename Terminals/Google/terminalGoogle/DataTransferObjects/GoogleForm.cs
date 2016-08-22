using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace terminalGoogle.DataTransferObjects
{
    public class GoogleForm
    {
        public GoogleForm()
        {
            FormFields = new List<GoogleFormField>();
        }

        [JsonProperty("formFields")]
        public List<GoogleFormField> FormFields { get; set; } 
    }
}