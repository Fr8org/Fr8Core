using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Utility
{
    public class Tag : BaseDO
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
