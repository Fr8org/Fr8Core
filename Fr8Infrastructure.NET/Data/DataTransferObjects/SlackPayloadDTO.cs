using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class SlackPayloadDTO
    {
        public string Channel { get; set; }

        public string Username { get; set; }

        public string Text { get; set; }

       

    }
}
